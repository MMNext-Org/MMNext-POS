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
    public class LicenseInfoService : ILicenseInfoService
    {
        private readonly ILicenseInfoRepository _repo;
        private readonly IAuditService _auditService;

        public LicenseInfoService(ILicenseInfoRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<LicenseInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<LicenseInfo?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.FirstOrDefault(l => l.LicenseKey == licenseKey);
        }

        public Task<IReadOnlyList<LicenseInfo>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<LicenseInfo> AddAsync(LicenseInfo entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(LicenseInfo), result.Id, "Create", null, result, 1, "System", $"Created license {result.LicenseKey}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(LicenseInfo entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(LicenseInfo), entity.Id, "Update", existing, entity, 1, "System", $"Updated license {entity.LicenseKey}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(LicenseInfo), id, "Delete", existing, null, 1, "System", $"Deleted license {existing?.LicenseKey ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
