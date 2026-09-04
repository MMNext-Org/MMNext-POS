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
    public class ReportMenusService : IReportMenusService
    {
        private readonly IReportMenusRepository _repo;
        private readonly IAuditService _auditService;

        public ReportMenusService(IReportMenusRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<ReportMenus?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<ReportMenus>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ReportMenus>> GetByParentCodeAsync(string parentCode, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(r => r.ParentCode == parentCode).OrderBy(r => r.DisplayOrder).ToList();
        }

        public async Task<ReportMenus> AddAsync(ReportMenus entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ReportMenus), result.Id, "Create", null, result, 1, "System", $"Created report menu {result.Code} - {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(ReportMenus entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ReportMenus), entity.Id, "Update", existing, entity, 1, "System", $"Updated report menu {entity.Code} - {entity.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ReportMenus), id, "Delete", existing, null, 1, "System", $"Deleted report menu {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
