using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IExpenseService
    {
        Task<Expense?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default);
        Task UpdateAsync(Expense expense, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
