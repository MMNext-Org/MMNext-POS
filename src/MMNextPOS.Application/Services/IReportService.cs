using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IReportService
    {
        // Report Menus
        Task<ReportMenus?> GetReportMenuByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReportMenus>> GetReportMenusAsync(bool includeReportsOnly = false, CancellationToken cancellationToken = default);
        Task<ReportMenus> AddReportMenuAsync(ReportMenus reportMenu, CancellationToken cancellationToken = default);
        Task UpdateReportMenuAsync(ReportMenus reportMenu, CancellationToken cancellationToken = default);
        Task DeleteReportMenuAsync(int id, CancellationToken cancellationToken = default);

        // Starman Reports
        Task<StarCashFlowReport?> GetCashFlowReportAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StarCashFlowReport>> GetCashFlowReportsAsync(int locationId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<StarProfitLossReport?> GetProfitLossReportAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StarProfitLossReport>> GetProfitLossReportsAsync(int locationId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<StarStockBalanceReport?> GetStockBalanceReportAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StarStockBalanceReport>> GetStockBalanceReportsAsync(int locationId, DateTime asOfDate, CancellationToken cancellationToken = default);
        Task<StarReorderReport?> GetReorderReportAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StarReorderReport>> GetReorderReportsAsync(int locationId, DateTime asOfDate, CancellationToken cancellationToken = default);
        Task<StarOutstandingReport?> GetOutstandingReportAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StarOutstandingReport>> GetOutstandingReportsAsync(int locationId, DateTime asOfDate, CancellationToken cancellationToken = default);

        // Generic Report Generation
        Task<byte[]> GenerateReportAsync(string reportName, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    }
}
