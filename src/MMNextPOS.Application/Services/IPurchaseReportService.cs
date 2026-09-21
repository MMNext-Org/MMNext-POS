using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IPurchaseReportService
    {
        // Purchase Invoice Report
        Task<byte[]> GeneratePurchaseInvoiceReportAsync(int purchaseId, CancellationToken cancellationToken = default);
        Task<byte[]> GeneratePurchaseInvoiceReportByNoAsync(string invoiceNo, CancellationToken cancellationToken = default);

        // Purchase History Report
        Task<byte[]> GeneratePurchaseHistoryReportAsync(int? supplierId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<byte[]> GeneratePurchaseByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

        // Purchase Summary Reports
        Task<byte[]> GeneratePurchaseSummaryByProductAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<byte[]> GeneratePurchaseSummaryByVendorAsync(int? vendorId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

        // Purchase Return Reports
        Task<byte[]> GeneratePurchaseReturnReportAsync(int returnId, CancellationToken cancellationToken = default);
        Task<byte[]> GeneratePurchaseReturnSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

        // Payment Register Report
        Task<byte[]> GeneratePaymentRegisterReportAsync(int? customerId, int? supplierId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

        // Expense Summary Report
        Task<byte[]> GenerateExpenseSummaryReportAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    }
}
