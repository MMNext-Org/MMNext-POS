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
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _repo;
        private readonly IPurchaseDetailRepository _detailRepo;
        private readonly IPurchaseReturnRepository _returnRepo;
        private readonly IPurchaseReturnDetailRepository _returnDetailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IStockMovementService _stockMovementService;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IOutstandingService _outstandingService;

        public PurchaseService(
            IPurchaseRepository repo,
            IPurchaseDetailRepository detailRepo,
            IPurchaseReturnRepository returnRepo,
            IPurchaseReturnDetailRepository returnDetailRepo,
            IProductRepository productRepo,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IStockMovementService stockMovementService,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IOutstandingService outstandingService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _returnRepo = returnRepo ?? throw new ArgumentNullException(nameof(returnRepo));
            _returnDetailRepo = returnDetailRepo ?? throw new ArgumentNullException(nameof(returnDetailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _invoiceNumberGenerator = invoiceNumberGenerator ?? throw new ArgumentNullException(nameof(invoiceNumberGenerator));
            _outstandingService = outstandingService ?? throw new ArgumentNullException(nameof(outstandingService));
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
            if (purchase == null) throw new ArgumentNullException(nameof(purchase));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var detailList = details as IList<PurchaseDetail> ?? details.ToList();
            if (detailList.Count == 0)
            {
                throw new ValidationException("A purchase must have at least one line item.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // 1) Aggregate duplicate lines (ProductId → sum Quantity, last UnitPrice wins).
                var aggregated = AggregateDuplicateLines(detailList);

                // 2) Round money per line (banker's half-even) and sum for the header.
                var rounded = RoundLines(aggregated);
                purchase.TotalAmount = rounded.Sum(l => l.UnitPrice * l.Quantity);
                purchase.NetAmount = purchase.TotalAmount - purchase.DiscountAmount + purchase.TaxAmount;
                if (purchase.PurchaseDate == default) purchase.PurchaseDate = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(purchase.Status)) purchase.Status = "Active";

                // 3) Atomic stock increase per line. TryIncrementStockAsync returns
                //    false only when the product does not exist; there is no
                //    "insufficient stock" failure mode for purchases.
                foreach (var d in rounded)
                {
                    var ok = await _productRepo.TryIncrementStockAsync(d.ProductId, d.Quantity, 1, "Purchase", cancellationToken).ConfigureAwait(false);
                    if (!ok)
                    {
                        throw new ValidationException($"Product {d.ProductId} not found.");
                    }
                }

                // 4) Persist purchase header + details.
                var created = await _repo.AddAsync(purchase, cancellationToken).ConfigureAwait(false);
                foreach (var d in rounded)
                {
                    d.PurchaseId = created.Id;
                    d.LineTotal = d.Quantity * d.UnitPrice - d.DiscountAmount + d.TaxAmount;
                    await _detailRepo.AddAsync(d, cancellationToken).ConfigureAwait(false);
                }

                // 5) Stock movement header + one detail per purchase line.
                await _stockMovementService.AddPurchaseMovementAsync(created, (IReadOnlyList<PurchaseDetail>)rounded, createdByUserId: null, cancellationToken).ConfigureAwait(false);

                // 6) Auto-generated purchase-order number (PUR-YYYY-NNNNNN) on a
                //    separate sequence from invoices.
                var poNo = await _invoiceNumberGenerator.NextAsync("PUR", cancellationToken).ConfigureAwait(false);
                created.InvoiceNo = poNo;
                await _repo.UpdateAsync(created, cancellationToken).ConfigureAwait(false);

                // 7) Supplier outstanding if NetAmount > PaidAmount.
                //    SupplierOutstanding uses CreditAmount for "amount owed to
                //    supplier" (opposite of CustomerOutstanding's AR convention).
                var owed = created.NetAmount - created.PaidAmount;
                if (created.SupplierId > 0 && owed > 0m)
                {
                    await _outstandingService.AddSupplierOutstandingAsync(new SupplierOutstanding
                    {
                        SupplierId = created.SupplierId,
                        PurchaseId = created.Id,
                        TransactionDate = created.PurchaseDate,
                        DebitAmount = 0m,
                        CreditAmount = owed,
                        Balance = owed,
                        Description = $"Auto from purchase {poNo}",
                        Status = "Open"
                    }, cancellationToken).ConfigureAwait(false);
                }

                // 8) Audit INSIDE the transaction.
                await _auditService.LogAsync(
                    entityName: nameof(Purchase),
                    entityId: created.Id,
                    action: "Create",
                    oldValues: null,
                    newValues: new
                    {
                        PurchaseId = created.Id,
                        PurchaseOrderNo = poNo,
                        SupplierId = created.SupplierId,
                        NetAmount = created.NetAmount,
                        LineCount = rounded.Count
                    },
                    userId: null,
                    userName: null,
                    description: $"Purchase {poNo} posted: {rounded.Count} lines, total {created.NetAmount:C2}",
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return created;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
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
            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var purchase = await _repo.GetByIdAsync(purchaseId, cancellationToken).ConfigureAwait(false);
                if (purchase == null)
                    throw new KeyNotFoundException($"Purchase {purchaseId} not found");

                if (purchase.Status == "Received" || purchase.Status == "Cancelled")
                    throw new InvalidOperationException($"Cannot receive purchase with status {purchase.Status}");

                var details = (await _detailRepo.GetAllAsync(cancellationToken).ConfigureAwait(false))
                    .Where(d => d.PurchaseId == purchaseId).ToList();

                foreach (var item in receivedItems)
                {
                    var detail = details.FirstOrDefault(d => d.Id == item.detailId);
                    if (detail == null) continue;

                    detail.ReceivedQuantity = item.receivedQuantity;
                    await _detailRepo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);

                    // Atomic stock increase (Phase 3 hardening replaces the old
                    // non-atomic AdjustStockAsync path used pre-Phase-2).
                    var ok = await _productRepo.TryIncrementStockAsync(
                        detail.ProductId, item.receivedQuantity, 1, $"Purchase Receive - {purchase.InvoiceNo}", cancellationToken).ConfigureAwait(false);
                    if (!ok)
                    {
                        throw new ValidationException($"Product {detail.ProductId} not found.");
                    }
                }

                purchase.Status = details.All(d => d.ReceivedQuantity >= d.Quantity) ? "Received" : "PartiallyReceived";

                await _repo.UpdateAsync(purchase, cancellationToken).ConfigureAwait(false);

                // Stock movement + audit inside the transaction.
                await _stockMovementService.AddPurchaseMovementAsync(purchase, details, receivedByUserId, cancellationToken).ConfigureAwait(false);
                await _auditService.LogAsync(
                    entityName: nameof(Purchase),
                    entityId: purchase.Id,
                    action: "Receive",
                    oldValues: null,
                    newValues: purchase,
                    userId: null,
                    userName: null,
                    description: $"Received purchase {purchase.InvoiceNo}",
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return purchase;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
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

        /// <summary>
        /// Combines repeated lines for the same product into one line. The
        /// last-seen UnitPrice wins, and quantities are summed. Throws if any
        /// line has a non-positive quantity.
        /// </summary>
        internal static IList<PurchaseDetail> AggregateDuplicateLines(IList<PurchaseDetail> input)
        {
            var byProduct = new Dictionary<int, PurchaseDetail>();
            var order = new List<int>();
            foreach (var d in input)
            {
                if (d.Quantity <= 0)
                {
                    throw new ValidationException("Line quantity must be positive.");
                }
                if (d.UnitPrice < 0m)
                {
                    throw new ValidationException("Line unit price must be non-negative.");
                }
                if (byProduct.TryGetValue(d.ProductId, out var existing))
                {
                    existing.Quantity += d.Quantity;
                    existing.UnitPrice = d.UnitPrice;
                }
                else
                {
                    byProduct[d.ProductId] = new PurchaseDetail
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice
                    };
                    order.Add(d.ProductId);
                }
            }
            return order.Select(pid => byProduct[pid]).ToList();
        }

        /// <summary>
        /// Rounds each line's monetary fields to 2 decimal places using
        /// banker's half-even (MidpointRounding.ToEven). Returns the same
        /// input list (in-place rounding).
        /// </summary>
        internal static IList<PurchaseDetail> RoundLines(IList<PurchaseDetail> input)
        {
            foreach (var d in input)
            {
                d.UnitPrice = Math.Round(d.UnitPrice, 2, MidpointRounding.ToEven);
                d.DiscountAmount = Math.Round(d.DiscountAmount, 2, MidpointRounding.ToEven);
                d.TaxAmount = Math.Round(d.TaxAmount, 2, MidpointRounding.ToEven);
            }
            return input;
        }
    }
}
