using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IDiscountService
    {
        Task<Discount?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Discount>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Discount> AddAsync(Discount discount, CancellationToken cancellationToken = default);
        Task UpdateAsync(Discount discount, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
