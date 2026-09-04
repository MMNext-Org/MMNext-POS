using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class IssueHeaderService : IIssueHeaderService
    {
        private readonly IIssueHeaderRepository _repo;
        private readonly IAuditService _auditService;

        public IssueHeaderService(IIssueHeaderRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<IssueHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<IssueHeader>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<IssueHeader> AddAsync(IssueHeader entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(IssueHeader), result.Id, "Create", null, result, 1, "System", $"Created issue header {result.IssueNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(IssueHeader entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(IssueHeader), entity.Id, "Update", existing, entity, 1, "System", $"Updated issue header {entity.IssueNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(IssueHeader), id, "Delete", existing, null, 1, "System", $"Deleted issue header {existing?.IssueNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
