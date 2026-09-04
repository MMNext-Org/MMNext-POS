using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class MobileClientService : IMobileClientService
    {
        private readonly IMobileClientRepository _repo;
        private readonly IAuditService _auditService;

        public MobileClientService(IMobileClientRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<MobileClient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<MobileClient>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<MobileClient> AddAsync(MobileClient entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(MobileClient), result.Id, "Create", null, result, 1, "System", $"Created mobile client {result.DeviceId}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(MobileClient entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(MobileClient), entity.Id, "Update", existing, entity, 1, "System", $"Updated mobile client {entity.DeviceId}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(MobileClient), id, "Delete", existing, null, 1, "System", $"Deleted mobile client {existing?.DeviceId ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
