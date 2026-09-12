using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISalesReportService
    {
        // Sales Invoice Report
        Task<byte[]> GenerateSalesInvoiceReportAsync(int saleId, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateSalesInvoiceReportByNoAsync(string invoiceNo, CancellationToken cancellationToken = default);

        // Sales History Report
        Task<byte[]> GenerateSalesHistoryReportAsync(int? customerId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateSalesByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

        // Sales Summary Reports
        Task<byte[]> GenerateSalesSummaryByProductAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateSalesSummaryBySupplierAsync(int? supplierId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

        // Sales Return Reports
        Task<byte[]> GenerateSalesReturnReportAsync(int returnId, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateSalesReturnSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

        // Outstanding Reports
        Task<byte[]> GenerateCustomerOutstandingReportAsync(int customerId, DateTime asOfDate, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateSupplierOutstandingReportAsync(int supplierId, DateTime asOfDate, CancellationToken cancellationToken = default);
    }
}