using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class StockTransferService : IStockTransferService
    {
        private readonly IStockTransferRepository _transferRepo;
        private readonly IStockTransferDetailRepository _detailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IStockMovementService _stockMovementService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public StockTransferService(
            IStockTransferRepository transferRepo,
            IStockTransferDetailRepository detailRepo,
            IProductRepository productRepo,
            IStockMovementService stockMovementService,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _transferRepo = transferRepo ?? throw new ArgumentNullException(nameof(transferRepo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<StockTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _transferRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<StockTransfer> CreateTransferAsync(StockTransfer transfer, IEnumerable<StockTransferDetail> details, int userId, CancellationToken cancellationToken = default)
        {
            if (transfer == null) throw new ArgumentNullException(nameof(transfer));
            if (details == null || !details.Any()) throw new ArgumentException("Transfer must have at least one line item.", nameof(details));

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Validate locations
                if (transfer.FromLocationId == transfer.ToLocationId)
                    throw new ValidationException("Source and destination locations must be different.");

                // Validate stock availability
                foreach (var detail in details)
                {
                    var product = await _productRepo.GetByIdAsync(detail.ProductId, cancellationToken);
                    if (product == null)
                        throw new ValidationException($"Product {detail.ProductId} not found.");
                    if (product.StockQuantity < detail.Quantity)
                        throw new ValidationException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {detail.Quantity}.");
                }

                // Generate transfer number
                transfer.TransferNo = await GenerateTransferNumberAsync(cancellationToken);
                transfer.Status = "Draft";
                transfer.CreatedByUserId = userId;
                transfer.TransferDate = DateTime.UtcNow;

                var created = await _transferRepo.AddAsync(transfer, cancellationToken);

                foreach (var detail in details)
                {
                    detail.StockTransferId = created.Id;
                    detail.ReceivedQuantity = 0; // Initially zero
                    await _detailRepo.AddAsync(detail, cancellationToken);
                }

                // Audit
                await _auditService.LogAsync(nameof(StockTransfer), created.Id, "Create", null, created, userId, "System", $"Created transfer {transfer.TransferNo}", cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return created;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<StockTransfer> ReleaseTransferAsync(int transferId, int userId, CancellationToken cancellationToken = default)
        {
            var transfer = await GetByIdAsync(transferId, cancellationToken);
            if (transfer == null)
                throw new KeyNotFoundException($"Transfer {transferId} not found.");

            if (transfer.Status != "Draft")
                throw new InvalidOperationException($"Cannot release transfer with status {transfer.Status}. Only Draft transfers can be released.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var details = await _detailRepo.GetAllAsync(cancellationToken);
                var transferDetails = details.Where(d => d.StockTransferId == transferId).ToList();

                // For each detail, decrement stock at source and create stock movement
                foreach (var detail in transferDetails)
                {
                    var product = await _productRepo.GetByIdAsync(detail.ProductId, cancellationToken);
                    if (product == null)
                        throw new ValidationException($"Product {detail.ProductId} not found.");

                    if (product.StockQuantity < detail.Quantity)
                        throw new ValidationException($"Insufficient stock for product {product.Name} at source location.");

                    var success = await _productRepo.TryDecrementStockAsync(detail.ProductId, detail.Quantity, userId, $"Transfer Out {transfer.TransferNo}", cancellationToken);
                    if (!success)
                        throw new ValidationException($"Failed to decrement stock for product {product.Name}.");

                    // Create stock movement for outbound transfer
                    await _stockMovementService.AddTransferOutMovementAsync(
                        transfer.FromLocationId, transfer.ToLocationId, detail.ProductId,
                        detail.Quantity, product.Price, $"Transfer to Location {transfer.ToLocationId}",
                        userId, transferId, cancellationToken);
                }

                transfer.Status = "InTransit";
                transfer.ReleasedDate = DateTime.UtcNow;
                transfer.ReleasedByUserId = userId;
                await _transferRepo.UpdateAsync(transfer, cancellationToken);

                // Audit
                await _auditService.LogAsync(nameof(StockTransfer), transferId, "Release", null, transfer, userId, "System", $"Released transfer {transfer.TransferNo}", cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return transfer;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<StockTransfer> ReceiveTransferAsync(int transferId, IEnumerable<(int detailId, int receivedQuantity, string? serialNumber)> receivedItems, int receivedByUserId, CancellationToken cancellationToken = default)
        {
            var transfer = await GetByIdAsync(transferId, cancellationToken);
            if (transfer == null)
                throw new KeyNotFoundException($"Transfer {transferId} not found.");

            if (transfer.Status != "InTransit" && transfer.Status != "PartiallyReceived")
                throw new InvalidOperationException($"Cannot receive transfer with status {transfer.Status}. Only InTransit or PartiallyReceived transfers can be received.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var details = await _detailRepo.GetAllAsync(cancellationToken);
                var transferDetails = details.Where(d => d.StockTransferId == transferId).ToList();

                foreach (var item in receivedItems)
                {
                    var detail = transferDetails.FirstOrDefault(d => d.Id == item.detailId);
                    if (detail == null)
                        continue;

                    var remainingQty = detail.Quantity - detail.ReceivedQuantity;
                    if (item.receivedQuantity > remainingQty)
                        throw new ValidationException($"Cannot receive more than remaining quantity ({remainingQty}) for product {detail.ProductId}.");

                    detail.ReceivedQuantity += item.receivedQuantity;
                    detail.SerialNumber = item.serialNumber;
                    await _detailRepo.UpdateAsync(detail, cancellationToken);

                    // Increment stock at destination
                    var product = await _productRepo.GetByIdAsync(detail.ProductId, cancellationToken);
                    if (product != null)
                    {
                        var success = await _productRepo.TryIncrementStockAsync(detail.ProductId, item.receivedQuantity, receivedByUserId, $"Transfer In {transfer.TransferNo}", cancellationToken);
                        if (!success)
                            throw new ValidationException($"Failed to increment stock for product {product.Name}.");
                    }

                    // Create stock movement for inbound transfer
                    await _stockMovementService.AddTransferInMovementAsync(
                        transfer.FromLocationId, transfer.ToLocationId, detail.ProductId,
                        item.receivedQuantity, product!.Price, $"Transfer from Location {transfer.FromLocationId}",
                        receivedByUserId, transferId, cancellationToken);
                }

                // Update status based on received quantities
                var allDetails = await _detailRepo.GetAllAsync(cancellationToken);
                var updatedDetails = allDetails.Where(d => d.StockTransferId == transferId).ToList();

                var allReceived = updatedDetails.All(d => d.ReceivedQuantity >= d.Quantity);
                if (allReceived)
                {
                    transfer.Status = "Received";
                    transfer.ReceivedDate = DateTime.UtcNow;
                    transfer.ReceivedByUserId = receivedByUserId;
                }
                else if (updatedDetails.Any(d => d.ReceivedQuantity > 0))
                {
                    transfer.Status = "PartiallyReceived";
                }

                await _transferRepo.UpdateAsync(transfer, cancellationToken);

                // Audit
                await _auditService.LogAsync(nameof(StockTransfer), transferId, "Receive", null, transfer, receivedByUserId, "System", $"Received transfer {transfer.TransferNo}", cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return transfer;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<StockTransfer> CancelTransferAsync(int transferId, int userId, string? reason, CancellationToken cancellationToken = default)
        {
            var transfer = await GetByIdAsync(transferId, cancellationToken);
            if (transfer == null)
                throw new KeyNotFoundException($"Transfer {transferId} not found.");

            if (transfer.Status == "Received" || transfer.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot cancel transfer with status {transfer.Status}.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var details = await _detailRepo.GetAllAsync(cancellationToken);
                var transferDetails = details.Where(d => d.StockTransferId == transferId).ToList();

                // If transfer was InTransit/PartiallyReceived, need to reverse stock
                if (transfer.Status == "InTransit" || transfer.Status == "PartiallyReceived")
                {
                    foreach (var detail in transferDetails)
                    {
                        // Reverse the shipment (decremented from source)
                        if (detail.ReceivedQuantity.HasValue && detail.ReceivedQuantity > 0)
                        {
                            var product = await _productRepo.GetByIdAsync(detail.ProductId, cancellationToken);
                            if (product != null)
                            {
                                // Increment back the received amount at destination
                                var success = await _productRepo.TryDecrementStockAsync(detail.ProductId, detail.ReceivedQuantity.Value, userId, $"Cancel Transfer {transfer.TransferNo}", cancellationToken);
                                if (!success)
                                {
                                    // If decrement fails, try to log but don't fail the cancel
                                }
                            }
                        }
                    }
                }

                transfer.Status = "Cancelled";
                transfer.CancelledDate = DateTime.UtcNow;
                transfer.CancelledByUserId = userId;
                transfer.CancelReason = reason;
                await _transferRepo.UpdateAsync(transfer, cancellationToken);

                // Audit
                await _auditService.LogAsync(nameof(StockTransfer), transferId, "Cancel", null, transfer, userId, "System", $"Cancelled transfer {transfer.TransferNo}: {reason ?? "No reason"}", cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return transfer;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<IReadOnlyList<StockTransfer>> GetTransfersAsync(int? fromLocationId = null, int? toLocationId = null, string? status = null, CancellationToken cancellationToken = default)
        {
            var all = await _transferRepo.GetAllAsync(cancellationToken);
            var result = all.AsQueryable();

            if (fromLocationId.HasValue)
                result = result.Where(t => t.FromLocationId == fromLocationId.Value);
            if (toLocationId.HasValue)
                result = result.Where(t => t.ToLocationId == toLocationId.Value);
            if (!string.IsNullOrEmpty(status))
                result = result.Where(t => t.Status == status);

            return result.OrderByDescending(t => t.TransferDate).ToList();
        }

        public async Task<IReadOnlyList<StockTransfer>> GetInTransitTransfersAsync(int? toLocationId = null, CancellationToken cancellationToken = default)
        {
            var all = await _transferRepo.GetAllAsync(cancellationToken);
            var result = all.Where(t => t.Status == "InTransit" || t.Status == "PartiallyReceived");

            if (toLocationId.HasValue)
                result = result.Where(t => t.ToLocationId == toLocationId.Value);

            return result.OrderBy(t => t.TransferDate).ToList();
        }

        public async Task<IReadOnlyList<StockTransferDetail>> GetTransferDetailsAsync(int transferId, CancellationToken cancellationToken = default)
        {
            var all = await _detailRepo.GetAllAsync(cancellationToken);
            return all.Where(d => d.StockTransferId == transferId).ToList();
        }

        public async Task<decimal> GetTransferCostAsync(int transferId, CancellationToken cancellationToken = default)
        {
            var details = await GetTransferDetailsAsync(transferId, cancellationToken);
            return details.Sum(d => d.Quantity * d.UnitPrice);
        }

        public async Task<string> GenerateTransferNumberAsync(CancellationToken cancellationToken)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"ST-{year}";
            var all = await _transferRepo.GetAllAsync(cancellationToken);
            var lastNumber = all
                .Where(t => t.TransferNo.StartsWith(prefix))
                .Select(t =>
                {
                    var parts = t.TransferNo.Split('-');
                    return parts.Length > 2 ? int.TryParse(parts[2], out var num) ? num : 0 : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            var nextNum = lastNumber + 1;
            return $"{prefix}-{nextNum:D6}";
        }
    }
}
