using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class AppInfoService : IAppInfoService
    {
        private readonly IAppInfoRepository _repo;
        private readonly IAuditService _auditService;

        public AppInfoService(IAppInfoRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<AppInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<AppInfo>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<AppInfo> AddAsync(AppInfo entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(AppInfo), result.Id, "Create", null, result, 1, "System", $"Created app info {result.AppName} v{result.Version}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(AppInfo entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(AppInfo), entity.Id, "Update", existing, entity, 1, "System", $"Updated app info {entity.AppName} v{entity.Version}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(AppInfo), id, "Delete", existing, null, 1, "System", $"Deleted app info {existing?.AppName ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
