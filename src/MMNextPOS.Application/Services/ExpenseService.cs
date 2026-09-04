using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repo;
        private readonly IAuditService _auditService;

        public ExpenseService(IExpenseRepository repo, IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(expense, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Expense), result.Id, "Create", null, result, 1, "System", $"Created expense {result.ExpenseNo}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(expense.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(expense, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Expense), expense.Id, "Update", existing, expense, 1, "System", $"Updated expense {expense.ExpenseNo}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(Expense), id, "Delete", existing, null, 1, "System", $"Deleted expense {existing?.ExpenseNo ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }
    }
}
