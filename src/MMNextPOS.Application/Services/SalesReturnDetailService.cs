using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class SalesReturnDetailService : ISalesReturnDetailService
    {
        private readonly ISalesReturnDetailRepository _repo;
        private readonly IAuditService _auditService;

        public SalesReturnDetailService(ISalesReturnDetailRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<IReadOnlyList<SalesReturnDetail>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public Task<SalesReturnDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<SalesReturnDetail> AddAsync(SalesReturnDetail salesReturnDetail, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(salesReturnDetail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SalesReturnDetail), result.Id, "Create", null, result, 1, "System", $"Created sales return detail {result.Id}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(SalesReturnDetail salesReturnDetail, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(salesReturnDetail.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(salesReturnDetail, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SalesReturnDetail), salesReturnDetail.Id, "Update", existing, salesReturnDetail, 1, "System", $"Updated sales return detail {salesReturnDetail.Id}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SalesReturnDetail), id, "Delete", existing, null, 1, "System", $"Deleted sales return detail {existing?.Id ?? id}", cancellationToken).ConfigureAwait(false);
        }
    }
}
