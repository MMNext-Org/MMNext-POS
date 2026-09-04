using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class ExpenseTypeService : IExpenseTypeService
    {
        private readonly IExpenseTypeRepository _repo;
        private readonly IAuditService _auditService;

        public ExpenseTypeService(IExpenseTypeRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<ExpenseType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<ExpenseType>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<ExpenseType> AddAsync(ExpenseType expenseType, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(expenseType, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ExpenseType), result.Id, "Create", null, result, 1, "System", $"Created expense type {result.Code} - {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(ExpenseType expenseType, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(expenseType.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(expenseType, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ExpenseType), expenseType.Id, "Update", existing, expenseType, 1, "System", $"Updated expense type {expenseType.Code} - {expenseType.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(ExpenseType), id, "Delete", existing, null, 1, "System", $"Deleted expense type {existing?.Code ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
