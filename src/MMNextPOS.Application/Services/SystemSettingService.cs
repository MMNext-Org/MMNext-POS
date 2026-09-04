using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly ISystemSettingRepository _repo;
        private readonly IAuditService _auditService;

        public SystemSettingService(
            ISystemSettingRepository repo,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            return _repo.GetByKeyAsync(key, cancellationToken);
        }

        public Task<SystemSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<SystemSetting>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public Task<IReadOnlyList<SystemSetting>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return _repo.GetByCategoryAsync(category, cancellationToken);
        }

        public Task<IReadOnlyList<SystemSetting>> GetSystemSettingsAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetSystemSettingsAsync(cancellationToken);
        }

        public async Task<SystemSetting> AddAsync(SystemSetting setting, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(setting, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SystemSetting), result.Id, "Create", null, result, 1, "System", $"Created system setting {result.Key}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(SystemSetting setting, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(setting.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(setting, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SystemSetting), setting.Id, "Update", existing, setting, 1, "System", $"Updated system setting {setting.Key}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SystemSetting), id, "Delete", existing, null, 1, "System", $"Deleted system setting {existing?.Key ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}