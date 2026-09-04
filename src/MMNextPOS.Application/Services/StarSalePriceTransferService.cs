using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class StarSalePriceTransferService : IStarSalePriceTransferService
    {
        private readonly IStarSalePriceTransferRepository _repo;
        private readonly IAuditService _auditService;

        public StarSalePriceTransferService(
            IStarSalePriceTransferRepository repo,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<StarSalePriceTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<StarSalePriceTransfer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<StarSalePriceTransfer> AddAsync(StarSalePriceTransfer entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(StarSalePriceTransfer), result.Id, "Create", null, result, 1, "System", $"Created sale price transfer {result.TransferNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(StarSalePriceTransfer entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(StarSalePriceTransfer), entity.Id, "Update", existing, entity, 1, "System", $"Updated sale price transfer {entity.TransferNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(StarSalePriceTransfer), id, "Delete", existing, null, 1, "System", $"Deleted sale price transfer {existing?.TransferNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
