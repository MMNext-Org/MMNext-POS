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
    public class ReportService : IReportService
    {
        private readonly IReportMenusRepository _reportMenuRepo;
        private readonly IStarCashFlowReportRepository _cashFlowRepo;
        private readonly IStarProfitLossReportRepository _profitLossRepo;
        private readonly IStarStockBalanceReportRepository _stockBalanceRepo;
        private readonly IStarReorderReportRepository _reorderRepo;
        private readonly IStarOutstandingReportRepository _outstandingRepo;
        private readonly IAuditService _auditService;

        public ReportService(
            IReportMenusRepository reportMenuRepo,
            IStarCashFlowReportRepository cashFlowRepo,
            IStarProfitLossReportRepository profitLossRepo,
            IStarStockBalanceReportRepository stockBalanceRepo,
            IStarReorderReportRepository reorderRepo,
            IStarOutstandingReportRepository outstandingRepo,
            IAuditService auditService)
        {
            _reportMenuRepo = reportMenuRepo ?? throw new ArgumentNullException(nameof(reportMenuRepo));
            _cashFlowRepo = cashFlowRepo ?? throw new ArgumentNullException(nameof(cashFlowRepo));
            _profitLossRepo = profitLossRepo ?? throw new ArgumentNullException(nameof(profitLossRepo));
            _stockBalanceRepo = stockBalanceRepo ?? throw new ArgumentNullException(nameof(stockBalanceRepo));
            _reorderRepo = reorderRepo ?? throw new ArgumentNullException(nameof(reorderRepo));
            _outstandingRepo = outstandingRepo ?? throw new ArgumentNullException(nameof(outstandingRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        // Report Menus
        public Task<ReportMenus?> GetReportMenuByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _reportMenuRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<ReportMenus>> GetReportMenusAsync(bool includeReportsOnly = false, CancellationToken cancellationToken = default)
        {
            var all = await _reportMenuRepo.GetAllAsync(cancellationToken);
            if (includeReportsOnly)
            {
                return all.Where(r => r.IsReport).OrderBy(r => r.DisplayOrder).ToList();
            }
            return all.OrderBy(r => r.DisplayOrder).ToList();
        }

        public async Task<ReportMenus> AddReportMenuAsync(ReportMenus reportMenu, CancellationToken cancellationToken = default)
        {
            var result = await _reportMenuRepo.AddAsync(reportMenu, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ReportMenus), result.Id, "Create", null, result, 1, "System", $"Created report menu {result.Code} - {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateReportMenuAsync(ReportMenus reportMenu, CancellationToken cancellationToken = default)
        {
            var existing = await _reportMenuRepo.GetByIdAsync(reportMenu.Id, cancellationToken).ConfigureAwait(false);
            await _reportMenuRepo.UpdateAsync(reportMenu, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ReportMenus), reportMenu.Id, "Update", existing, reportMenu, 1, "System", $"Updated report menu {reportMenu.Code} - {reportMenu.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteReportMenuAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _reportMenuRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _reportMenuRepo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ReportMenus), id, "Delete", existing, null, 1, "System", $"Deleted report menu {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Starman Reports
        public Task<StarCashFlowReport?> GetCashFlowReportAsync(int id, CancellationToken cancellationToken = default)
        {
            return _cashFlowRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<StarCashFlowReport>> GetCashFlowReportsAsync(int locationId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            var all = await _cashFlowRepo.GetAllAsync(cancellationToken);
            return all.Where(r => r.LocationId == locationId && r.ReportDate >= fromDate && r.ReportDate <= toDate)
                      .OrderBy(r => r.ReportDate)
                      .ToList();
        }

        public Task<StarProfitLossReport?> GetProfitLossReportAsync(int id, CancellationToken cancellationToken = default)
        {
            return _profitLossRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<StarProfitLossReport>> GetProfitLossReportsAsync(int locationId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            var all = await _profitLossRepo.GetAllAsync(cancellationToken);
            return all.Where(r => r.LocationId == locationId && r.FromDate >= fromDate && r.ToDate <= toDate)
                      .OrderBy(r => r.FromDate)
                      .ToList();
        }

        public Task<StarStockBalanceReport?> GetStockBalanceReportAsync(int id, CancellationToken cancellationToken = default)
        {
            return _stockBalanceRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<StarStockBalanceReport>> GetStockBalanceReportsAsync(int locationId, DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            var all = await _stockBalanceRepo.GetAllAsync(cancellationToken);
            return all.Where(r => r.LocationId == locationId && r.LastMovementDate <= asOfDate)
                      .OrderBy(r => r.ProductName)
                      .ToList();
        }

        public Task<StarReorderReport?> GetReorderReportAsync(int id, CancellationToken cancellationToken = default)
        {
            return _reorderRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<StarReorderReport>> GetReorderReportsAsync(int locationId, DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            var all = await _reorderRepo.GetAllAsync(cancellationToken);
            return all.Where(r => r.LocationId == locationId)
                      .OrderBy(r => r.ProductName)
                      .ToList();
        }

        public Task<StarOutstandingReport?> GetOutstandingReportAsync(int id, CancellationToken cancellationToken = default)
        {
            return _outstandingRepo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<StarOutstandingReport>> GetOutstandingReportsAsync(int locationId, DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            var all = await _outstandingRepo.GetAllAsync(cancellationToken);
            return all.Where(r => r.LocationId == locationId && r.AsOfDate <= asOfDate)
                      .OrderBy(r => r.PartyName)
                      .ToList();
        }

        // Generic Report Generation
        public async Task<byte[]> GenerateReportAsync(string reportName, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            // Base implementation returns JSON representation
            var reportData = new
            {
                ReportName = reportName,
                GeneratedAt = DateTime.UtcNow,
                Parameters = parameters
            };

            var json = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
}
