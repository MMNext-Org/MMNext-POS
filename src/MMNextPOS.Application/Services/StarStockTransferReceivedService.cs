using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class StarStockTransferReceivedService : IStarStockTransferReceivedService
    {
        private readonly IStarStockTransferReceivedRepository _repo;
        private readonly IAuditService _auditService;

        public StarStockTransferReceivedService(
            IStarStockTransferReceivedRepository repo,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<StarStockTransferReceived?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<StarStockTransferReceived>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<StarStockTransferReceived> AddAsync(StarStockTransferReceived entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(StarStockTransferReceived), result.Id, "Create", null, result, 1, "System", $"Created stock transfer received {result.TransferNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(StarStockTransferReceived entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(StarStockTransferReceived), entity.Id, "Update", existing, entity, 1, "System", $"Updated stock transfer received {entity.TransferNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(StarStockTransferReceived), id, "Delete", existing, null, 1, "System", $"Deleted stock transfer received {existing?.TransferNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
