using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IPurchaseReturnRepository _repo;
        private readonly IPurchaseReturnDetailRepository _detailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockMovementService _stockMovementService;
        private readonly IAuditService _auditService;

        public PurchaseReturnService(
            IPurchaseReturnRepository repo,
            IPurchaseReturnDetailRepository detailRepo,
            IProductRepository productRepo,
            IUnitOfWork unitOfWork,
            IStockMovementService stockMovementService,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<PurchaseReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<PurchaseReturn>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<PagedResult<PurchaseReturn>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _repo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<PurchaseReturn> AddAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(purchaseReturn, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturn), result.Id, "Create", null, result, 1, "System", $"Created purchase return {result.ReturnNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(purchaseReturn.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(purchaseReturn, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturn), purchaseReturn.Id, "Update", existing, purchaseReturn, 1, "System", $"Updated purchase return {purchaseReturn.ReturnNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (existing == null)
            {
                throw new ValidationException($"Purchase return {id} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // Restore stock for the return lines being deleted.
                var details = await _detailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
                var returnDetails = details.Where(d => d.PurchaseReturnId == id).ToList();
                foreach (var d in returnDetails)
                {
                    await _productRepo.TryIncrementStockAsync(d.ProductId, d.Quantity, 1, "PurchaseReturnDelete", cancellationToken).ConfigureAwait(false);
                    await _detailRepo.DeleteAsync(d.Id, cancellationToken).ConfigureAwait(false);
                }

                await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);

                await _auditService.LogAsync(
                    nameof(PurchaseReturn),
                    id,
                    "Delete",
                    existing,
                    null,
                    1,
                    "System",
                    $"Deleted purchase return {existing.ReturnNo ?? id.ToString()} and restored stock",
                    cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        // Purchase Return Details
        public async Task<IReadOnlyList<PurchaseReturnDetail>> GetPurchaseReturnDetailsAsync(int purchaseReturnId, CancellationToken cancellationToken = default)
        {
            var all = await _detailRepo.GetAllAsync(cancellationToken);
            return all.Where(d => d.PurchaseReturnId == purchaseReturnId).ToList();
        }

        public async Task<PurchaseReturnDetail> AddPurchaseReturnDetailAsync(PurchaseReturnDetail detail, CancellationToken cancellationToken = default)
        {
            var result = await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturnDetail), result.Id, "Create", null, result, 1, "System", $"Created purchase return detail", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdatePurchaseReturnDetailAsync(PurchaseReturnDetail detail, CancellationToken cancellationToken = default)
        {
            var existing = await _detailRepo.GetByIdAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            await _detailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturnDetail), detail.Id, "Update", existing, detail, 1, "System", $"Updated purchase return detail", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeletePurchaseReturnDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _detailRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _detailRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturnDetail), id, "Delete", existing, null, 1, "System", $"Deleted purchase return detail {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }

        // Purchase Return with details
        public async Task<PurchaseReturn> CreatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default)
        {
            if (returnOrder == null) throw new ArgumentNullException(nameof(returnOrder));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var detailList = details as IList<PurchaseReturnDetail> ?? details.ToList();
            if (detailList.Count == 0)
            {
                throw new ValidationException("A purchase return must have at least one line item.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // 1) Decrement stock for every returned line. Insufficient stock or
                //    unknown product fails the whole transaction before any write.
                foreach (var d in detailList)
                {
                    var ok = await _productRepo.TryDecrementStockAsync(d.ProductId, d.Quantity, 1, "PurchaseReturn", cancellationToken).ConfigureAwait(false);
                    if (!ok)
                    {
                        throw new ValidationException($"Product {d.ProductId} not found or insufficient stock to return {d.Quantity} unit(s).");
                    }
                }

                // 2) Persist return header + details.
                var result = await _repo.AddAsync(returnOrder, cancellationToken).ConfigureAwait(false);
                foreach (var d in detailList)
                {
                    d.PurchaseReturnId = result.Id;
                    await _detailRepo.AddAsync(d, cancellationToken).ConfigureAwait(false);
                }

                // 3) Stock movement header + one detail row per return line (MovementType = "Return").
                foreach (var d in detailList)
                {
                    await _stockMovementService.AddReturnMovementAsync(
                        result.Id,
                        d.ProductId,
                        d.Quantity,
                        unitCost: 0m,
                        locationId: null,
                        createdByUserId: null,
                        reason: $"Purchase return {result.ReturnNo}",
                        cancellationToken).ConfigureAwait(false);
                }

                // 4) Audit inside the transaction.
                await _auditService.LogAsync(
                    nameof(PurchaseReturn),
                    result.Id,
                    "Create",
                    null,
                    returnOrder,
                    1,
                    "System",
                    $"Created purchase return {result.ReturnNo} with {detailList.Count} line(s)",
                    cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<PurchaseReturn> UpdatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default)
        {
            if (returnOrder == null) throw new ArgumentNullException(nameof(returnOrder));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var detailList = details as IList<PurchaseReturnDetail> ?? details.ToList();
            if (detailList.Count == 0)
            {
                throw new ValidationException("A purchase return must have at least one line item.");
            }

            var existing = await _repo.GetByIdAsync(returnOrder.Id, cancellationToken).ConfigureAwait(false);
            if (existing == null)
            {
                throw new ValidationException($"Purchase return {returnOrder.Id} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // 1) Reverse stock for the previously recorded return lines.
                var existingDetails = await _detailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
                var previousDetails = existingDetails.Where(d => d.PurchaseReturnId == returnOrder.Id).ToList();
                foreach (var d in previousDetails)
                {
                    await _productRepo.TryIncrementStockAsync(d.ProductId, d.Quantity, 1, "PurchaseReturnReversal", cancellationToken).ConfigureAwait(false);
                }

                // 2) Decrement stock for the new return lines.
                foreach (var d in detailList)
                {
                    var ok = await _productRepo.TryDecrementStockAsync(d.ProductId, d.Quantity, 1, "PurchaseReturn", cancellationToken).ConfigureAwait(false);
                    if (!ok)
                    {
                        throw new ValidationException($"Product {d.ProductId} not found or insufficient stock to return {d.Quantity} unit(s).");
                    }
                }

                // 3) Replace details.
                await _repo.UpdateAsync(returnOrder, cancellationToken).ConfigureAwait(false);
                foreach (var d in previousDetails)
                {
                    await _detailRepo.DeleteAsync(d.Id, cancellationToken).ConfigureAwait(false);
                }
                foreach (var d in detailList)
                {
                    d.PurchaseReturnId = returnOrder.Id;
                    await _detailRepo.AddAsync(d, cancellationToken).ConfigureAwait(false);
                }

                // 4) Audit inside the transaction.
                await _auditService.LogAsync(
                    nameof(PurchaseReturn),
                    returnOrder.Id,
                    "Update",
                    existing,
                    returnOrder,
                    1,
                    "System",
                    $"Updated purchase return {returnOrder.ReturnNo} with {detailList.Count} line(s)",
                    cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return returnOrder;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        // Stock validation
        public async Task<int> GetAvailableStockAsync(int productId, CancellationToken cancellationToken = default)
        {
            var product = await _productRepo.GetByIdAsync(productId, cancellationToken).ConfigureAwait(false);
            return product?.StockQuantity ?? 0;
        }

        public async Task<bool> HasSufficientStockAsync(int productId, int quantity, CancellationToken cancellationToken = default)
        {
            var available = await GetAvailableStockAsync(productId, cancellationToken).ConfigureAwait(false);
            return available >= quantity;
        }
    }
}
