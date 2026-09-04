using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class PCUpdateService : IPCUpdateService
    {
        private readonly IPCUpdateRepository _repo;
        private readonly IAuditService _auditService;

        public PCUpdateService(IPCUpdateRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<PCUpdate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<PCUpdate>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<PCUpdate> AddAsync(PCUpdate entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PCUpdate), result.Id, "Create", null, result, 1, "System", $"Created PC update {result.Version}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(PCUpdate entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PCUpdate), entity.Id, "Update", existing, entity, 1, "System", $"Updated PC update {entity.Version}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PCUpdate), id, "Delete", existing, null, 1, "System", $"Deleted PC update {existing?.Version ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
