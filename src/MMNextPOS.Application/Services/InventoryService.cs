using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IStockMovementRepository _stockMovementRepo;
        private readonly IStockMovementDetailRepository _stockMovementDetailRepo;
        private readonly IAssemblyRepository _assemblyRepo;
        private readonly IAssemblyDetailRepository _assemblyDetailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IStockTransferRepository _stockTransferRepo;
        private readonly IStockTransferDetailRepository _stockTransferDetailRepo;
        private readonly IAuditService _auditService;

        public InventoryService(
            IStockMovementRepository stockMovementRepo,
            IStockMovementDetailRepository stockMovementDetailRepo,
            IAssemblyRepository assemblyRepo,
            IAssemblyDetailRepository assemblyDetailRepo,
            IProductRepository productRepo,
            IStockTransferRepository stockTransferRepo,
            IStockTransferDetailRepository stockTransferDetailRepo,
            IAuditService auditService)
        {
            _stockMovementRepo = stockMovementRepo ?? throw new ArgumentNullException(nameof(stockMovementRepo));
            _stockMovementDetailRepo = stockMovementDetailRepo ?? throw new ArgumentNullException(nameof(stockMovementDetailRepo));
            _assemblyRepo = assemblyRepo ?? throw new ArgumentNullException(nameof(assemblyRepo));
            _assemblyDetailRepo = assemblyDetailRepo ?? throw new ArgumentNullException(nameof(assemblyDetailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _stockTransferRepo = stockTransferRepo ?? throw new ArgumentNullException(nameof(stockTransferRepo));
            _stockTransferDetailRepo = stockTransferDetailRepo ?? throw new ArgumentNullException(nameof(stockTransferDetailRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        // Stock Movement
        public Task<StockMovement?> GetStockMovementByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _stockMovementRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<StockMovement>> GetStockMovementsAsync(int? locationId = null, string? movementType = null, CancellationToken cancellationToken = default)
        {
            var all = await _stockMovementRepo.GetAllAsync(cancellationToken);
            var result = all.AsQueryable();

            if (locationId.HasValue)
                result = result.Where(m => m.LocationId == locationId.Value);
            if (!string.IsNullOrEmpty(movementType))
                result = result.Where(m => m.MovementType == movementType);

            return result.OrderByDescending(m => m.MovementDate).ToList();
        }

        public async Task<PagedResult<StockMovement>> GetStockMovementsPageAsync(int page, int pageSize, int? locationId = null, string? movementType = null, CancellationToken cancellationToken = default)
        {
            return await _stockMovementRepo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<StockMovement> AddStockMovementAsync(StockMovement movement, IEnumerable<StockMovementDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await _stockMovementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            foreach (var detail in details)
            {
                detail.StockMovementId = result.Id;
                await _stockMovementDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
                await UpdateProductStockFromDetailAsync(detail, cancellationToken);
            }

            await _auditService.LogAsync(nameof(StockMovement), result.Id, "Create", null, movement, 1, "System", $"Created stock movement {result.MovementNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateStockMovementAsync(StockMovement movement, IEnumerable<StockMovementDetail> details, CancellationToken cancellationToken = default)
        {
            var existing = await _stockMovementRepo.GetByIdAsync(movement.Id, cancellationToken).ConfigureAwait(false);
            await _stockMovementRepo.UpdateAsync(movement, cancellationToken).ConfigureAwait(false);

            var existingDetails = await _stockMovementDetailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var movementDetails = existingDetails.Where(d => d.StockMovementId == movement.Id).ToList();
            foreach (var detail in movementDetails)
            {
                await _stockMovementDetailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            }

            foreach (var detail in details)
            {
                detail.StockMovementId = movement.Id;
                await _stockMovementDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
                await UpdateProductStockFromDetailAsync(detail, cancellationToken);
            }

            await _auditService.LogAsync(nameof(StockMovement), movement.Id, "Update", null, movement, 1, "System", $"Updated stock movement {movement.MovementNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteStockMovementAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _stockMovementRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

            var allDetails = await _stockMovementDetailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var movementDetails = allDetails.Where(d => d.StockMovementId == id).ToList();
            foreach (var detail in movementDetails)
            {
                await ReverseProductStockFromDetailAsync(detail, cancellationToken);
                await _stockMovementDetailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            }

            await _stockMovementRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(StockMovement), id, "Delete", null, null, 1, "System", $"Deleted stock movement {existing?.MovementNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Stock Movement Details
        public async Task<IReadOnlyList<StockMovementDetail>> GetStockMovementDetailsAsync(int stockMovementId, CancellationToken cancellationToken = default)
        {
            var all = await _stockMovementDetailRepo.GetAllAsync(cancellationToken);
            return all.Where(d => d.StockMovementId == stockMovementId).ToList();
        }

        public async Task<StockMovementDetail> AddStockMovementDetailAsync(StockMovementDetail detail, CancellationToken cancellationToken = default)
        {
            var result = await _stockMovementDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            await UpdateProductStockFromDetailAsync(detail, cancellationToken);
            await _auditService.LogAsync(nameof(StockMovementDetail), result.Id, "Create", null, result, 1, "System", $"Created stock movement detail", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateStockMovementDetailAsync(StockMovementDetail detail, CancellationToken cancellationToken = default)
        {
            var existing = await _stockMovementDetailRepo.GetByIdAsync(detail.Id, cancellationToken).ConfigureAwait(false);

            await ReverseProductStockFromDetailAsync(existing, cancellationToken);
            await _stockMovementDetailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);
            await UpdateProductStockFromDetailAsync(detail, cancellationToken);

            await _auditService.LogAsync(nameof(StockMovementDetail), detail.Id, "Update", existing, detail, 1, "System", $"Updated stock movement detail", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteStockMovementDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _stockMovementDetailRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await ReverseProductStockFromDetailAsync(existing, cancellationToken);
            await _stockMovementDetailRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(StockMovementDetail), id, "Delete", existing, null, 1, "System", $"Deleted stock movement detail {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }

        // Assembly (BOM)
        public Task<Assembly?> GetAssemblyByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _assemblyRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Assembly>> GetAssembliesAsync(CancellationToken cancellationToken = default)
        {
            return _assemblyRepo.GetAllAsync(cancellationToken);
        }

        public async Task<Assembly> AddAssemblyAsync(Assembly assembly, IEnumerable<AssemblyDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await _assemblyRepo.AddAsync(assembly, cancellationToken).ConfigureAwait(false);

            foreach (var detail in details)
            {
                detail.AssemblyId = result.Id;
                await _assemblyDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(Assembly), result.Id, "Create", null, assembly, 1, "System", $"Created assembly {result.AssemblyNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAssemblyAsync(Assembly assembly, IEnumerable<AssemblyDetail> details, CancellationToken cancellationToken = default)
        {
            var existing = await _assemblyRepo.GetByIdAsync(assembly.Id, cancellationToken).ConfigureAwait(false);
            await _assemblyRepo.UpdateAsync(assembly, cancellationToken).ConfigureAwait(false);

            var existingDetails = await _assemblyDetailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var assemblyDetails = existingDetails.Where(d => d.AssemblyId == assembly.Id).ToList();
            foreach (var detail in assemblyDetails)
            {
                await _assemblyDetailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            }

            foreach (var detail in details)
            {
                detail.AssemblyId = assembly.Id;
                await _assemblyDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(Assembly), assembly.Id, "Update", null, assembly, 1, "System", $"Updated assembly {assembly.AssemblyNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAssemblyAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _assemblyRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _assemblyRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Assembly), id, "Delete", null, null, 1, "System", $"Deleted assembly {existing?.AssemblyNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Assembly Details
        public async Task<IReadOnlyList<AssemblyDetail>> GetAssemblyDetailsAsync(int assemblyId, CancellationToken cancellationToken = default)
        {
            var all = await _assemblyDetailRepo.GetAllAsync(cancellationToken);
            return all.Where(d => d.AssemblyId == assemblyId).ToList();
        }

        public async Task<AssemblyDetail> AddAssemblyDetailAsync(AssemblyDetail detail, CancellationToken cancellationToken = default)
        {
            var result = await _assemblyDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(AssemblyDetail), result.Id, "Create", null, result, 1, "System", $"Created assembly detail", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAssemblyDetailAsync(AssemblyDetail detail, CancellationToken cancellationToken = default)
        {
            var existing = await _assemblyDetailRepo.GetByIdAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            await _assemblyDetailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(AssemblyDetail), detail.Id, "Update", existing, detail, 1, "System", $"Updated assembly detail", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAssemblyDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _assemblyDetailRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _assemblyDetailRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(AssemblyDetail), id, "Delete", existing, null, 1, "System", $"Deleted assembly detail {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }

        // Stock Transfers
        public Task<StockTransfer?> GetStockTransferByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _stockTransferRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<StockTransfer>> GetStockTransfersAsync(int? fromLocationId = null, int? toLocationId = null, string? status = null, CancellationToken cancellationToken = default)
        {
            var all = await _stockTransferRepo.GetAllAsync(cancellationToken);
            var result = all.AsQueryable();

            if (fromLocationId.HasValue)
                result = result.Where(t => t.FromLocationId == fromLocationId.Value);
            if (toLocationId.HasValue)
                result = result.Where(t => t.ToLocationId == toLocationId.Value);
            if (!string.IsNullOrEmpty(status))
                result = result.Where(t => t.Status == status);

            return result.OrderByDescending(t => t.TransferDate).ToList();
        }

        public async Task<PagedResult<StockTransfer>> GetStockTransfersPageAsync(int page, int pageSize, int? fromLocationId = null, int? toLocationId = null, string? status = null, CancellationToken cancellationToken = default)
        {
            return await _stockTransferRepo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<StockTransfer> AddStockTransferAsync(StockTransfer transfer, IEnumerable<StockTransferDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await _stockTransferRepo.AddAsync(transfer, cancellationToken).ConfigureAwait(false);

            foreach (var detail in details)
            {
                detail.StockTransferId = result.Id;
                await _stockTransferDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(StockTransfer), result.Id, "Create", null, transfer, 1, "System", $"Created stock transfer {result.TransferNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateStockTransferAsync(StockTransfer transfer, IEnumerable<StockTransferDetail> details, CancellationToken cancellationToken = default)
        {
            var existing = await _stockTransferRepo.GetByIdAsync(transfer.Id, cancellationToken).ConfigureAwait(false);
            await _stockTransferRepo.UpdateAsync(transfer, cancellationToken).ConfigureAwait(false);

            var existingDetails = await _stockTransferDetailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var transferDetails = existingDetails.Where(d => d.StockTransferId == transfer.Id).ToList();
            foreach (var detail in transferDetails)
            {
                await _stockTransferDetailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            }

            foreach (var detail in details)
            {
                detail.StockTransferId = transfer.Id;
                await _stockTransferDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(StockTransfer), transfer.Id, "Update", null, transfer, 1, "System", $"Updated stock transfer {transfer.TransferNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task ReceiveStockTransferAsync(int transferId, int receivedByUserId, IEnumerable<(int detailId, int receivedQuantity, string? serialNumber)> receivedItems, CancellationToken cancellationToken = default)
        {
            var transfer = await _stockTransferRepo.GetByIdAsync(transferId, cancellationToken).ConfigureAwait(false);
            if (transfer == null)
                throw new KeyNotFoundException($"Stock transfer {transferId} not found");

            if (transfer.Status == "Received" || transfer.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot receive transfer with status {transfer.Status}");

            transfer.Status = "Received";
            transfer.ReceivedDate = DateTime.UtcNow;
            transfer.ReceivedByUserId = receivedByUserId;
            await _stockTransferRepo.UpdateAsync(transfer, cancellationToken).ConfigureAwait(false);

            var details = await _stockTransferDetailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var transferDetails = details.Where(d => d.StockTransferId == transferId).ToList();

            foreach (var item in receivedItems)
            {
                var detail = transferDetails.FirstOrDefault(d => d.Id == item.detailId);
                if (detail != null)
                {
                    detail.ReceivedQuantity = item.receivedQuantity;
                    if (!string.IsNullOrEmpty(item.serialNumber))
                    {
                        detail.SerialNumber = item.serialNumber;
                    }
                    await _stockTransferDetailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);

                    await UpdateProductStockAtLocationAsync(detail.ProductId, transfer.ToLocationId, item.receivedQuantity, cancellationToken);
                }
            }

            var allReceived = transferDetails.All(d => d.ReceivedQuantity >= d.Quantity);
            if (allReceived)
            {
                transfer.Status = "Received";
            }
            else
            {
                transfer.Status = "PartiallyReceived";
            }
            await _stockTransferRepo.UpdateAsync(transfer, cancellationToken).ConfigureAwait(false);

            await _auditService.LogAsync(nameof(StockTransfer), transfer.Id, "Receive", null, transfer, 1, "System", $"Received stock transfer {transfer.TransferNo}", cancellationToken).ConfigureAwait(false);
        }

        // Serial Number Tracking
        public async Task<IReadOnlyList<StockMovementDetail>> GetSerialTrackedItemsAsync(int productId, int? locationId = null, CancellationToken cancellationToken = default)
        {
            var allDetails = await _stockMovementDetailRepo.GetAllAsync(cancellationToken);
            var result = allDetails.Where(d => d.ProductId == productId && !string.IsNullOrEmpty(d.SerialNumber)).ToList();

            if (locationId.HasValue)
            {
                var movementIds = result.Select(d => d.StockMovementId).ToList();
                var movements = await _stockMovementRepo.GetAllAsync(cancellationToken);
                var movementIdsWithLocation = movements.Where(m => m.LocationId == locationId && movementIds.Contains(m.Id)).Select(m => m.Id).ToList();
                result = result.Where(d => movementIdsWithLocation.Contains(d.StockMovementId)).ToList();
            }

            return result;
        }

        public async Task<StockMovementDetail?> GetSerialDetailAsync(string serialNumber, CancellationToken cancellationToken = default)
        {
            var allDetails = await _stockMovementDetailRepo.GetAllAsync(cancellationToken);
            return allDetails.FirstOrDefault(d => d.SerialNumber == serialNumber);
        }

        // Stock Validation & Queries
        public async Task<int> GetAvailableStockAsync(int productId, int? locationId = null, CancellationToken cancellationToken = default)
        {
            var product = await _productRepo.GetByIdAsync(productId, cancellationToken).ConfigureAwait(false);
            return product?.StockQuantity ?? 0;
        }

        public async Task<bool> HasSufficientStockAsync(int productId, int quantity, int? locationId = null, CancellationToken cancellationToken = default)
        {
            var available = await GetAvailableStockAsync(productId, locationId, cancellationToken);
            return available >= quantity;
        }

        public async Task<decimal> GetAverageCostAsync(int productId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepo.GetByIdAsync(productId, cancellationToken).ConfigureAwait(false);
            return product?.Price ?? 0m;
        }

        // Private helper methods
        private async Task UpdateProductStockFromDetailAsync(StockMovementDetail detail, CancellationToken cancellationToken)
        {
            var movement = await _stockMovementRepo.GetByIdAsync(detail.StockMovementId, cancellationToken);
            if (movement == null) return;

            var adjustment = movement.MovementType switch
            {
                "Receive" => detail.Quantity,
                "Purchase" => detail.Quantity,
                "Return" => detail.Quantity,
                "Issue" => -detail.Quantity,
                "Sale" => -detail.Quantity,
                "Damaged" => -detail.Quantity,
                "Lost" => -detail.Quantity,
                "Adjust" => detail.Quantity,
                _ => 0
            };

            if (adjustment != 0)
            {
                await _productRepo.AdjustStockAsync(detail.ProductId, adjustment,
                    $"{movement.MovementType} - {movement.MovementNo}", 1, cancellationToken);
            }
        }

        private async Task ReverseProductStockFromDetailAsync(StockMovementDetail? detail, CancellationToken cancellationToken)
        {
            if (detail == null) return;

            var movement = await _stockMovementRepo.GetByIdAsync(detail.StockMovementId, cancellationToken);
            if (movement == null) return;

            var adjustment = movement.MovementType switch
            {
                "Receive" => -detail.Quantity,
                "Purchase" => -detail.Quantity,
                "Return" => -detail.Quantity,
                "Issue" => detail.Quantity,
                "Sale" => detail.Quantity,
                "Damaged" => detail.Quantity,
                "Lost" => detail.Quantity,
                "Adjust" => -detail.Quantity,
                _ => 0
            };

            if (adjustment != 0)
            {
                await _productRepo.AdjustStockAsync(detail.ProductId, adjustment,
                    $"Reverse {movement.MovementType} - {movement.MovementNo}", 1, cancellationToken);
            }
        }

        private async Task UpdateProductStockAtLocationAsync(int productId, int locationId, int quantity, CancellationToken cancellationToken)
        {
            await _productRepo.AdjustStockAsync(productId, quantity,
                $"Stock Transfer Receive at Location {locationId}", 1, cancellationToken);
        }
    }
}
