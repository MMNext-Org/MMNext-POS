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
    public class PurchaseDetailService : IPurchaseDetailService
    {
        private readonly IPurchaseDetailRepository _repo;
        private readonly IAuditService _auditService;

        public PurchaseDetailService(IPurchaseDetailRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<PurchaseDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<PurchaseDetail>> GetByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken);
            return all.Where(d => d.PurchaseId == purchaseId).ToList();
        }

        public async Task<PurchaseDetail> AddAsync(PurchaseDetail detail, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseDetail), result.Id, "Create", null, result, 1, "System", $"Created purchase detail for purchase {detail.PurchaseId}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(PurchaseDetail detail, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(detail.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(detail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseDetail), detail.Id, "Update", existing, detail, 1, "System", $"Updated purchase detail {detail.Id}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(PurchaseDetail), id, "Delete", existing, null, 1, "System", $"Deleted purchase detail {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }
    }
}
