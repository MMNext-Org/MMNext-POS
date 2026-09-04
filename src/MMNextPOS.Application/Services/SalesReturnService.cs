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
    public class SalesReturnService : ISalesReturnService
    {
        private readonly ISalesReturnRepository _repo;
        private readonly IAuditService _auditService;

        public SalesReturnService(ISalesReturnRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<SalesReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<SalesReturn>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<PagedResult<SalesReturn>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _repo.GetPageAsync(page, pageSize, cancellationToken);
        }

        public async Task<SalesReturn> AddAsync(SalesReturn salesReturn, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(salesReturn, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SalesReturn), result.Id, "Create", null, result, 1, "System", $"Created sales return {result.ReturnNo} for customer {result.CustomerId}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(SalesReturn salesReturn, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(salesReturn.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(salesReturn, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SalesReturn), salesReturn.Id, "Update", existing, salesReturn, 1, "System", $"Updated sales return {salesReturn.ReturnNo} for customer {salesReturn.CustomerId}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(SalesReturn), id, "Delete", existing, null, 1, "System", $"Deleted sales return {existing?.ReturnNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
