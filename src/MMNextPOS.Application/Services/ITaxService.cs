using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ITaxService
    {
        Task<Tax?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Tax>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Tax> AddAsync(Tax tax, CancellationToken cancellationToken = default);
        Task UpdateAsync(Tax tax, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
