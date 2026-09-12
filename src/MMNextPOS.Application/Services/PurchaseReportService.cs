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
    public class PurchaseReportService : IPurchaseReportService
    {
        private readonly IReportMenusRepository _reportMenuRepo;
        private readonly IPurchaseRepository _purchaseRepo;
        private readonly IPurchaseDetailRepository _purchaseDetailRepo;
        private readonly IPurchaseReturnRepository _purchaseReturnRepo;
        private readonly IPurchaseReturnDetailRepository _purchaseReturnDetailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IAuditService _auditService;

        public PurchaseReportService(
            IReportMenusRepository reportMenuRepo,
            IPurchaseRepository purchaseRepo,
            IPurchaseDetailRepository purchaseDetailRepo,
            IPurchaseReturnRepository purchaseReturnRepo,
            IPurchaseReturnDetailRepository purchaseReturnDetailRepo,
            IProductRepository productRepo,
            IAuditService auditService)
        {
            _reportMenuRepo = reportMenuRepo ?? throw new ArgumentNullException(nameof(reportMenuRepo));
            _purchaseRepo = purchaseRepo ?? throw new ArgumentNullException(nameof(purchaseRepo));
            _purchaseDetailRepo = purchaseDetailRepo ?? throw new ArgumentNullException(nameof(purchaseDetailRepo));
            _purchaseReturnRepo = purchaseReturnRepo ?? throw new ArgumentNullException(nameof(purchaseReturnRepo));
            _purchaseReturnDetailRepo = purchaseReturnDetailRepo ?? throw new ArgumentNullException(nameof(purchaseReturnDetailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<byte[]> GeneratePurchaseInvoiceReportAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), purchaseId, "GeneratePurchaseInvoiceReport", null, null, 1, "System", $"Generated purchase invoice report for purchase {purchaseId}", cancellationToken).ConfigureAwait(false);

            var purchase = await _purchaseRepo.GetByIdAsync(purchaseId, cancellationToken);
            if (purchase == null)
            {
                throw new KeyNotFoundException($"Purchase {purchaseId} not found");
            }

            var details = await _purchaseDetailRepo.GetAllAsync(cancellationToken);
            var purchaseDetails = details.Where(d => d.PurchaseId == purchaseId).ToList();

            // Get product names for all details
            var productIds = purchaseDetails.Select(d => d.ProductId).Distinct().ToList();
            var products = new Dictionary<int, string>();
            foreach (var productId in productIds)
            {
                var product = await _productRepo.GetByIdAsync(productId, cancellationToken);
                products[productId] = product?.Name ?? "Unknown";
            }

            var reportData = new
            {
                ReportTitle = "Purchase Invoice Report",
                InvoiceNo = purchase.InvoiceNo,
                PurchaseDate = purchase.PurchaseDate,
                SupplierId = purchase.SupplierId,
                TotalAmount = purchase.TotalAmount,
                DiscountAmount = purchase.DiscountAmount,
                TaxAmount = purchase.TaxAmount,
                NetAmount = purchase.NetAmount,
                Status = purchase.Status,
                LineItems = purchaseDetails.Select(d => new
                {
                    ProductName = products.TryGetValue(d.ProductId, out var name) ? name : "Unknown",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    DiscountAmount = d.DiscountAmount,
                    TaxAmount = d.TaxAmount,
                    LineTotal = d.LineTotal
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GeneratePurchaseInvoiceReportByNoAsync(string invoiceNo, CancellationToken cancellationToken = default)
        {
            var reportData = new
            {
                ReportTitle = "Purchase Invoice Report",
                InvoiceNo = invoiceNo,
                GeneratedAt = DateTime.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GeneratePurchaseHistoryReportAsync(int? supplierId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), supplierId ?? 0, "GeneratePurchaseHistoryReport", null, null, 1, "System", $"Generated purchase history report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var allPurchases = await _purchaseRepo.GetAllAsync(cancellationToken);

            var purchases = allPurchases.AsQueryable()
                .Where(p => supplierId.HasValue ? p.SupplierId == supplierId.Value : true)
                .Where(p => p.PurchaseDate >= fromDate && p.PurchaseDate <= toDate)
                .OrderByDescending(p => p.PurchaseDate)
                .Take(50)
                .ToList();

            var reportData = new
            {
                ReportTitle = "Purchase History Report",
                FromDate = fromDate,
                ToDate = toDate,
                TotalPurchases = purchases.Count,
                TotalAmount = purchases.Sum(p => p.TotalAmount),
                Purchases = purchases.Select(p => new
                {
                    InvoiceNo = p.InvoiceNo,
                    PurchaseDate = p.PurchaseDate,
                    SupplierId = p.SupplierId,
                    TotalAmount = p.TotalAmount
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GeneratePurchaseByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), 0, "GeneratePurchaseByDateRangeReport", null, null, 1, "System", $"Generated purchase by date range report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var allPurchases = await _purchaseRepo.GetAllAsync(cancellationToken);

            var purchases = allPurchases.AsQueryable()
                .Where(p => p.PurchaseDate >= fromDate && p.PurchaseDate <= toDate)
                .OrderByDescending(p => p.PurchaseDate)
                .ToList();

            var reportData = new
            {
                ReportTitle = "Purchase by Date Range Report",
                FromDate = fromDate,
                ToDate = toDate,
                TotalPurchases = purchases.Count,
                TotalAmount = purchases.Sum(p => p.TotalAmount),
                Purchases = purchases.Select(p => new
                {
                    InvoiceNo = p.InvoiceNo,
                    PurchaseDate = p.PurchaseDate,
                    SupplierId = p.SupplierId,
                    TotalAmount = p.TotalAmount
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GeneratePurchaseSummaryByProductAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), 0, "GeneratePurchaseSummaryByProductReport", null, null, 1, "System", $"Generated purchase summary by product report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var allPurchases = await _purchaseRepo.GetAllAsync(cancellationToken);
            var relevantPurchases = allPurchases.Where(p => p.PurchaseDate >= fromDate && p.PurchaseDate <= toDate).ToList();
            
            // Get purchase details for relevant purchases
            var purchaseIds = relevantPurchases.Select(p => p.Id).ToList();
            var allDetails = await _purchaseDetailRepo.GetAllAsync(cancellationToken);
            var relevantDetails = allDetails.Where(d => purchaseIds.Contains(d.PurchaseId)).ToList();

            // Get product names for all details
            var productIds = relevantDetails.Select(d => d.ProductId).Distinct().ToList();
            var products = new Dictionary<int, string>();
            foreach (var productId in productIds)
            {
                var product = await _productRepo.GetByIdAsync(productId, cancellationToken);
                products[productId] = product?.Name ?? "Unknown";
            }

            var summaryByProduct = relevantDetails
                .GroupBy(d => products.TryGetValue(d.ProductId, out var name) ? name : "Unknown")
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(d => d.Quantity),
                    TotalAmount = g.Sum(d => d.LineTotal)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            var reportData = new
            {
                ReportTitle = "Purchase Summary by Product Report",
                FromDate = fromDate,
                ToDate = toDate,
                GroupBy = "Product",
                Items = summaryByProduct
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GeneratePurchaseSummaryByVendorAsync(int? vendorId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), vendorId ?? 0, "GeneratePurchaseSummaryByVendorReport", null, null, 1, "System", $"Generated purchase summary by vendor report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var allPurchases = await _purchaseRepo.GetAllAsync(cancellationToken);
            var query = allPurchases
                .Where(p => p.PurchaseDate >= fromDate && p.PurchaseDate <= toDate);

            if (vendorId.HasValue)
                query = query.Where(p => p.SupplierId == vendorId.Value);

            var purchases = query.ToList();

            var summary = purchases
                .GroupBy(p => p.SupplierId)
                .Select(g => new
                {
                    SupplierId = g.Key,
                    TotalPurchases = g.Count(),
                    TotalAmount = g.Sum(p => p.TotalAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            var reportData = new
            {
                ReportTitle = "Purchase Summary by Vendor Report",
                FromDate = fromDate,
                ToDate = toDate,
                VendorId = vendorId,
                Items = summary
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GeneratePurchaseReturnReportAsync(int returnId, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), returnId, "GeneratePurchaseReturnReport", null, null, 1, "System", $"Generated purchase return report for return {returnId}", cancellationToken).ConfigureAwait(false);

            var purchaseReturn = await _purchaseReturnRepo.GetByIdAsync(returnId, cancellationToken);
            
            var details = await _purchaseReturnDetailRepo.GetAllAsync(cancellationToken);
            var returnDetails = details.Where(d => d.PurchaseReturnId == returnId).ToList();

            // Get product names for all details
            var productIds = returnDetails.Select(d => d.ProductId).Distinct().ToList();
            var products = new Dictionary<int, string>();
            foreach (var productId in productIds)
            {
                var product = await _productRepo.GetByIdAsync(productId, cancellationToken);
                products[productId] = product?.Name ?? "Unknown";
            }

            var reportData = new
            {
                ReportTitle = "Purchase Return Report",
                ReturnNo = purchaseReturn?.ReturnNo ?? "",
                ReturnDate = purchaseReturn?.ReturnDate ?? DateTime.MinValue,
                SupplierId = purchaseReturn?.SupplierId ?? 0,
                TotalAmount = purchaseReturn?.TotalAmount ?? 0m,
                Status = purchaseReturn?.Status ?? "",
                LineItems = returnDetails.Select(d => new
                {
                    ProductName = products.TryGetValue(d.ProductId, out var name) ? name : "Unknown",
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    LineTotal = d.LineTotal
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GeneratePurchaseReturnSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), 0, "GeneratePurchaseReturnSummaryReport", null, null, 1, "System", $"Generated purchase return summary report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var allReturns = await _purchaseReturnRepo.GetAllAsync(cancellationToken);
            var returns = allReturns
                .Where(r => r.ReturnDate >= fromDate && r.ReturnDate <= toDate)
                .ToList();

            var reportData = new
            {
                ReportTitle = "Purchase Return Summary Report",
                FromDate = fromDate,
                ToDate = toDate,
                TotalReturns = returns.Count,
                TotalRefunded = returns.Sum(r => r.TotalAmount),
                Items = returns.Select(r => new
                {
                    ReturnNo = r.ReturnNo,
                    ReturnDate = r.ReturnDate,
                    SupplierId = r.SupplierId,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GeneratePaymentRegisterReportAsync(int? customerId, int? supplierId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), 0, "GeneratePaymentRegisterReport", null, null, 1, "System", $"Generated payment register report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var reportData = new
            {
                ReportTitle = "Payment Register Report",
                FromDate = fromDate,
                ToDate = toDate,
                CustomerId = customerId,
                SupplierId = supplierId,
                TotalPayments = 0,
                TotalAmount = 0m,
                Entries = new List<object>()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateExpenseSummaryReportAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(PurchaseReportService), 0, "GenerateExpenseSummaryReport", null, null, 1, "System", $"Generated expense summary report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var reportData = new
            {
                ReportTitle = "Expense Summary Report",
                FromDate = fromDate,
                ToDate = toDate,
                TotalExpenses = 0,
                TotalAmount = 0m,
                ByCategory = new List<object>()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
}