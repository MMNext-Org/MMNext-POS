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
    public class SalesReportService : ISalesReportService
    {
        private readonly IReportMenusRepository _reportMenuRepo;
        private readonly ISaleRepository _saleRepo;
        private readonly ICustomerOutstandingRepository _customerOutstandingRepo;
        private readonly ISupplierOutstandingRepository _supplierOutstandingRepo;
        private readonly IPurchaseReturnRepository _purchaseReturnRepo;
        private readonly IAuditService _auditService;

        public SalesReportService(
            IReportMenusRepository reportMenuRepo,
            ISaleRepository saleRepo,
            ICustomerOutstandingRepository customerOutstandingRepo,
            ISupplierOutstandingRepository supplierOutstandingRepo,
            IPurchaseReturnRepository purchaseReturnRepo,
            IAuditService auditService)
        {
            _reportMenuRepo = reportMenuRepo ?? throw new ArgumentNullException(nameof(reportMenuRepo));
            _saleRepo = saleRepo ?? throw new ArgumentNullException(nameof(saleRepo));
            _customerOutstandingRepo = customerOutstandingRepo ?? throw new ArgumentNullException(nameof(customerOutstandingRepo));
            _supplierOutstandingRepo = supplierOutstandingRepo ?? throw new ArgumentNullException(nameof(supplierOutstandingRepo));
            _purchaseReturnRepo = purchaseReturnRepo ?? throw new ArgumentNullException(nameof(purchaseReturnRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<byte[]> GenerateSalesInvoiceReportAsync(int saleId, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), saleId, "GenerateSalesInvoiceReport", null, null, 1, "System", $"Generated sales invoice report for sale {saleId}", cancellationToken).ConfigureAwait(false);

            var sale = await _saleRepo.GetByIdAsync(saleId, cancellationToken);
            if (sale == null)
            {
                throw new KeyNotFoundException($"Sale {saleId} not found");
            }

            var reportData = new
            {
                ReportTitle = "Sales Invoice Report",
                InvoiceNo = sale.InvoiceNo,
                SaleDate = sale.SaleDate,
                CustomerId = sale.CustomerId,
                TotalAmount = sale.TotalAmount,
                DiscountAmount = 0m,
                TaxAmount = 0m,
                NetAmount = 0m,
                PaidAmount = 0m,
                Status = sale.Status,
                LineItems = new List<object>()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateSalesInvoiceReportByNoAsync(string invoiceNo, CancellationToken cancellationToken = default)
        {
            // Find sale by invoice number - would need a repository method
            // For now, return generic report
            var reportData = new
            {
                ReportTitle = "Sales Invoice Report",
                InvoiceNo = invoiceNo,
                GeneratedAt = DateTime.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateSalesHistoryReportAsync(int? customerId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), customerId ?? 0, "GenerateSalesHistoryReport", null, null, 1, "System", $"Generated sales history report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var allSales = await _saleRepo.GetAllAsync(cancellationToken);

            var sales = allSales
                .Where(s => (!customerId.HasValue || s.CustomerId == customerId.Value)
                           && s.SaleDate >= fromDate && s.SaleDate <= toDate)
                .OrderByDescending(s => s.SaleDate)
                .Take(50)
                .ToList();

            var reportData = new
            {
                ReportTitle = "Sales History Report",
                FromDate = fromDate,
                ToDate = toDate,
                CustomerId = customerId,
                TotalSales = sales.Count,
                TotalRevenue = sales.Sum(s => s.TotalAmount),
                Sales = sales.Select(s => new
                {
                    InvoiceNo = s.InvoiceNo,
                    SaleDate = s.SaleDate,
                    CustomerId = s.CustomerId,
                    TotalAmount = s.TotalAmount,
                    Status = s.Status
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateSalesByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), 0, "GenerateSalesByDateRangeReport", null, null, 1, "System", $"Generated sales by date range report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var allSales = await _saleRepo.GetAllAsync(cancellationToken);

            var sales = allSales
                .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate)
                .OrderByDescending(s => s.SaleDate)
                .ToList();

            var reportData = new
            {
                ReportTitle = "Sales by Date Range Report",
                FromDate = fromDate,
                ToDate = toDate,
                TotalSales = sales.Count,
                TotalRevenue = sales.Sum(s => s.TotalAmount),
                Sales = sales.Select(s => new
                {
                    InvoiceNo = s.InvoiceNo,
                    SaleDate = s.SaleDate,
                    CustomerId = s.CustomerId,
                    TotalAmount = s.TotalAmount
                }).ToList()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateSalesSummaryByProductAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), 0, "GenerateSalesSummaryByProductReport", null, null, 1, "System", $"Generated sales summary by product report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            // This would typically query sale details aggregated by product
            // For now, return structure
            var reportData = new
            {
                ReportTitle = "Sales Summary by Product Report",
                FromDate = fromDate,
                ToDate = toDate,
                GroupBy = "Product",
                Items = new List<object>()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateSalesSummaryBySupplierAsync(int? supplierId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), supplierId ?? 0, "GenerateSalesSummaryBySupplierReport", null, null, 1, "System", $"Generated sales summary by supplier report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var reportData = new
            {
                ReportTitle = "Sales Summary by Supplier Report",
                FromDate = fromDate,
                ToDate = toDate,
                SupplierId = supplierId,
                Items = new List<object>()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateSalesReturnReportAsync(int returnId, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), returnId, "GenerateSalesReturnReport", null, null, 1, "System", $"Generated sales return report for return {returnId}", cancellationToken).ConfigureAwait(false);

            var saleReturn = await _purchaseReturnRepo.GetByIdAsync(returnId, cancellationToken);

            var reportData = new
            {
                ReportTitle = "Sales Return Report",
                ReturnNo = saleReturn?.ReturnNo ?? "",
                ReturnDate = saleReturn?.ReturnDate ?? DateTime.MinValue,
                SupplierId = saleReturn?.SupplierId ?? 0,
                TotalAmount = saleReturn?.TotalAmount ?? 0m,
                Status = saleReturn?.Status ?? ""
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateSalesReturnSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), 0, "GenerateSalesReturnSummaryReport", null, null, 1, "System", $"Generated sales return summary report from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var reportData = new
            {
                ReportTitle = "Sales Return Summary Report",
                FromDate = fromDate,
                ToDate = toDate,
                TotalReturns = 0,
                TotalRefunded = 0m,
                Items = new List<object>()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateCustomerOutstandingReportAsync(int customerId, DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), customerId, "GenerateCustomerOutstandingReport", null, null, 1, "System", $"Generated customer outstanding report for customer {customerId} as of {asOfDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var outstanding = await _customerOutstandingRepo.GetByIdAsync(customerId, cancellationToken);

            var reportData = new
            {
                ReportTitle = "Customer Outstanding Report",
                CustomerId = customerId,
                AsOfDate = asOfDate,
                OutstandingBalance = outstanding?.Balance ?? 0m,
                TotalDebit = outstanding?.DebitAmount ?? 0m,
                TotalCredit = outstanding?.CreditAmount ?? 0m,
                Description = outstanding?.Description
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> GenerateSupplierOutstandingReportAsync(int supplierId, DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            await _auditService.LogAsync(nameof(SalesReportService), supplierId, "GenerateSupplierOutstandingReport", null, null, 1, "System", $"Generated supplier outstanding report for supplier {supplierId} as of {asOfDate:yyyy-MM-dd}", cancellationToken).ConfigureAwait(false);

            var outstanding = await _supplierOutstandingRepo.GetByIdAsync(supplierId, cancellationToken);

            var reportData = new
            {
                ReportTitle = "Supplier Outstanding Report",
                SupplierId = supplierId,
                AsOfDate = asOfDate,
                OutstandingBalance = outstanding?.Balance ?? 0m,
                TotalDebit = outstanding?.DebitAmount ?? 0m,
                TotalCredit = outstanding?.CreditAmount ?? 0m,
                Description = outstanding?.Description
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
}