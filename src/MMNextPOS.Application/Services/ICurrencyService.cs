using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ICurrencyService
    {
        Task<Currency?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Currency> AddAsync(Currency currency, CancellationToken cancellationToken = default);
        Task UpdateAsync(Currency currency, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}