using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class DeviceRequestService : IDeviceRequestService
    {
        private readonly IDeviceRequestRepository _repo;
        private readonly IAuditService _auditService;

        public DeviceRequestService(IDeviceRequestRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<DeviceRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<DeviceRequest>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<DeviceRequest> AddAsync(DeviceRequest entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(DeviceRequest), result.Id, "Create", null, result, 1, "System", $"Created device request {result.DeviceId}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(DeviceRequest entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(DeviceRequest), entity.Id, "Update", existing, entity, 1, "System", $"Updated device request {entity.DeviceId}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(DeviceRequest), id, "Delete", existing, null, 1, "System", $"Deleted device request {existing?.DeviceId ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
