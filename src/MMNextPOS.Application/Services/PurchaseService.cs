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
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _repo;
        private readonly IPurchaseDetailRepository _detailRepo;
        private readonly IPurchaseReturnRepository _returnRepo;
        private readonly IPurchaseReturnDetailRepository _returnDetailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IAuditService _auditService;

        public PurchaseService(
            IPurchaseRepository repo,
            IPurchaseDetailRepository detailRepo,
            IPurchaseReturnRepository returnRepo,
            IPurchaseReturnDetailRepository returnDetailRepo,
            IProductRepository productRepo,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _returnRepo = returnRepo ?? throw new ArgumentNullException(nameof(returnRepo));
            _returnDetailRepo = returnDetailRepo ?? throw new ArgumentNullException(nameof(returnDetailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<Purchase?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Purchase>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<PagedResult<Purchase>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _repo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<Purchase> AddAsync(Purchase purchase, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(purchase, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Purchase), result.Id, "Create", null, result, 1, "System", $"Created purchase {result.InvoiceNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(Purchase purchase, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(purchase.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(purchase, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Purchase), purchase.Id, "Update", existing, purchase, 1, "System", $"Updated purchase {purchase.InvoiceNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Purchase), id, "Delete", existing, null, 1, "System", $"Deleted purchase {existing?.InvoiceNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Purchase Details
        public async Task<IReadOnlyList<PurchaseDetail>> GetPurchaseDetailsAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            var all = await _detailRepo.GetAllAsync(cancellationToken);
            return all.Where(d => d.PurchaseId == purchaseId).ToList();
        }

        public async Task<PurchaseDetail> AddPurchaseDetailAsync(PurchaseDetail detail, CancellationToken cancellationToken = default)
        {
            var result = await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseDetail), result.Id, "Create", null, result, 1, "System", $"Created purchase detail", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdatePurchaseDetailAsync(PurchaseDetail detail, CancellationToken cancellationToken = default)
        {
            var existing = await _detailRepo.GetByIdAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            await _detailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseDetail), detail.Id, "Update", existing, detail, 1, "System", $"Updated purchase detail", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeletePurchaseDetailAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _detailRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _detailRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseDetail), id, "Delete", existing, null, 1, "System", $"Deleted purchase detail {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }

        // Purchase with details
        public async Task<Purchase> CreatePurchaseWithDetailsAsync(Purchase purchase, IEnumerable<PurchaseDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(purchase, cancellationToken).ConfigureAwait(false);

            foreach (var detail in details)
            {
                detail.PurchaseId = result.Id;
                await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(Purchase), result.Id, "Create", null, purchase, 1, "System", $"Created purchase {result.InvoiceNo} with details", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task<Purchase> UpdatePurchaseWithDetailsAsync(Purchase purchase, IEnumerable<PurchaseDetail> details, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(purchase.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(purchase, cancellationToken).ConfigureAwait(false);

            var existingDetails = await _detailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var purchaseDetails = existingDetails.Where(d => d.PurchaseId == purchase.Id).ToList();
            foreach (var detail in purchaseDetails)
            {
                await _detailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            }

            foreach (var detail in details)
            {
                detail.PurchaseId = purchase.Id;
                await _detailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(Purchase), purchase.Id, "Update", null, purchase, 1, "System", $"Updated purchase {purchase.InvoiceNo} with details", cancellationToken).ConfigureAwait(false);
            return purchase;
        }

        // Purchase lifecycle
        public async Task<Purchase> ReceivePurchaseAsync(int purchaseId, int receivedByUserId, IEnumerable<(int detailId, int receivedQuantity)> receivedItems, CancellationToken cancellationToken = default)
        {
            var purchase = await _repo.GetByIdAsync(purchaseId, cancellationToken).ConfigureAwait(false);
            if (purchase == null)
                throw new KeyNotFoundException($"Purchase {purchaseId} not found");

            if (purchase.Status == "Received" || purchase.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot receive purchase with status {purchase.Status}");

            var details = await _detailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var purchaseDetails = details.Where(d => d.PurchaseId == purchaseId).ToList();

            foreach (var item in receivedItems)
            {
                var detail = purchaseDetails.FirstOrDefault(d => d.Id == item.detailId);
                if (detail != null)
                {
                    detail.ReceivedQuantity = item.receivedQuantity;
                    await _detailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);

                    // Update product stock
                    await _productRepo.AdjustStockAsync(detail.ProductId, item.receivedQuantity,
                        $"Purchase Receive - {purchase.InvoiceNo}", 1, cancellationToken).ConfigureAwait(false);
                }
            }

            var allReceived = purchaseDetails.All(d => d.ReceivedQuantity >= d.Quantity);
            if (allReceived)
            {
                purchase.Status = "Received";
            }
            else
            {
                purchase.Status = "PartiallyReceived";
            }

            await _repo.UpdateAsync(purchase, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Purchase), purchase.Id, "Receive", null, purchase, 1, "System", $"Received purchase {purchase.InvoiceNo}", cancellationToken).ConfigureAwait(false);

            return purchase;
        }

        public async Task<Purchase> HoldPurchaseAsync(int purchaseId, int userId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var purchase = await _repo.GetByIdAsync(purchaseId, cancellationToken).ConfigureAwait(false);
            if (purchase == null)
                throw new KeyNotFoundException($"Purchase {purchaseId} not found");

            if (purchase.Status == "Received" || purchase.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot hold purchase with status {purchase.Status}");

            purchase.Status = "Hold";
            await _repo.UpdateAsync(purchase, cancellationToken).ConfigureAwait(false);

            await _auditService.LogAsync(nameof(Purchase), purchase.Id, "Hold", null, purchase, 1, "System", $"Held purchase {purchase.InvoiceNo}: {reason ?? "No reason provided"}", cancellationToken).ConfigureAwait(false);

            return purchase;
        }

        public async Task<Purchase> ReleasePurchaseAsync(int purchaseId, int userId, CancellationToken cancellationToken = default)
        {
            var purchase = await _repo.GetByIdAsync(purchaseId, cancellationToken).ConfigureAwait(false);
            if (purchase == null)
                throw new KeyNotFoundException($"Purchase {purchaseId} not found");

            if (purchase.Status != "Hold")
                throw new InvalidOperationException($"Cannot release purchase with status {purchase.Status}");

            purchase.Status = "Active";
            await _repo.UpdateAsync(purchase, cancellationToken).ConfigureAwait(false);

            await _auditService.LogAsync(nameof(Purchase), purchase.Id, "Release", null, purchase, 1, "System", $"Released purchase {purchase.InvoiceNo}", cancellationToken).ConfigureAwait(false);

            return purchase;
        }

        public async Task<Purchase> CancelPurchaseAsync(int purchaseId, int userId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var purchase = await _repo.GetByIdAsync(purchaseId, cancellationToken).ConfigureAwait(false);
            if (purchase == null)
                throw new KeyNotFoundException($"Purchase {purchaseId} not found");

            if (purchase.Status == "Received" || purchase.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot cancel purchase with status {purchase.Status}");

            purchase.Status = "Cancelled";
            await _repo.UpdateAsync(purchase, cancellationToken).ConfigureAwait(false);

            await _auditService.LogAsync(nameof(Purchase), purchase.Id, "Cancel", null, purchase, 1, "System", $"Cancelled purchase {purchase.InvoiceNo}: {reason ?? "No reason provided"}", cancellationToken).ConfigureAwait(false);

            return purchase;
        }

        // Purchase Returns
        public async Task<PurchaseReturn?> GetPurchaseReturnByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _returnRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<PurchaseReturn>> GetPurchaseReturnsAsync(int? supplierId = null, string? status = null, CancellationToken cancellationToken = default)
        {
            var all = await _returnRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var result = all.AsQueryable();

            if (supplierId.HasValue)
                result = result.Where(r => r.SupplierId == supplierId.Value);
            if (!string.IsNullOrEmpty(status))
                result = result.Where(r => r.Status == status);

            return result.OrderByDescending(r => r.ReturnDate).ToList();
        }

        public async Task<PurchaseReturn> CreatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await _returnRepo.AddAsync(returnOrder, cancellationToken).ConfigureAwait(false);

            foreach (var detail in details)
            {
                detail.PurchaseReturnId = result.Id;
                await _returnDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(PurchaseReturn), result.Id, "Create", null, returnOrder, 1, "System", $"Created purchase return {result.ReturnNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task<PurchaseReturn> UpdatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default)
        {
            var existing = await _returnRepo.GetByIdAsync(returnOrder.Id, cancellationToken).ConfigureAwait(false);
            await _returnRepo.UpdateAsync(returnOrder, cancellationToken).ConfigureAwait(false);

            var existingDetails = await _returnDetailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var returnDetails = existingDetails.Where(d => d.PurchaseReturnId == returnOrder.Id).ToList();
            foreach (var detail in returnDetails)
            {
                await _returnDetailRepo.DeleteAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            }

            foreach (var detail in details)
            {
                detail.PurchaseReturnId = returnOrder.Id;
                await _returnDetailRepo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            }

            await _auditService.LogAsync(nameof(PurchaseReturn), returnOrder.Id, "Update", null, returnOrder, 1, "System", $"Updated purchase return {returnOrder.ReturnNo}", cancellationToken).ConfigureAwait(false);
            return returnOrder;
        }

        public async Task<PurchaseReturn> ReceivePurchaseReturnAsync(int returnId, int receivedByUserId, IEnumerable<(int detailId, int receivedQuantity)> receivedItems, CancellationToken cancellationToken = default)
        {
            var returnOrder = await _returnRepo.GetByIdAsync(returnId, cancellationToken).ConfigureAwait(false);
            if (returnOrder == null)
                throw new KeyNotFoundException($"Purchase return {returnId} not found");

            if (returnOrder.Status == "Received" || returnOrder.Status == "Cancelled")
                throw new InvalidOperationException($"Cannot receive return with status {returnOrder.Status}");

            var details = await _returnDetailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var returnDetails = details.Where(d => d.PurchaseReturnId == returnId).ToList();

            foreach (var item in receivedItems)
            {
                var detail = returnDetails.FirstOrDefault(d => d.Id == item.detailId);
                if (detail != null)
                {
                    detail.ReceivedQuantity = item.receivedQuantity;
                    await _returnDetailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);

                    // Return stock to supplier (increase stock back)
                    await _productRepo.AdjustStockAsync(detail.ProductId, -item.receivedQuantity,
                        $"Purchase Return Receive - {returnOrder.ReturnNo}", 1, cancellationToken).ConfigureAwait(false);
                }
            }

            var allReceived = returnDetails.All(d => d.ReceivedQuantity >= d.Quantity);
            if (allReceived)
            {
                returnOrder.Status = "Received";
            }
            else
            {
                returnOrder.Status = "PartiallyReceived";
            }

            await _returnRepo.UpdateAsync(returnOrder, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturn), returnOrder.Id, "Receive", null, returnOrder, 1, "System", $"Received purchase return {returnOrder.ReturnNo}", cancellationToken).ConfigureAwait(false);

            return returnOrder;
        }

        public async Task DeletePurchaseReturnAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _returnRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _returnRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseReturn), id, "Delete", existing, null, 1, "System", $"Deleted purchase return {existing?.ReturnNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

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
