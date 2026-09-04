using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IExpenseTypeService
    {
        Task<ExpenseType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ExpenseType>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ExpenseType> AddAsync(ExpenseType expenseType, CancellationToken cancellationToken = default);
        Task UpdateAsync(ExpenseType expenseType, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
