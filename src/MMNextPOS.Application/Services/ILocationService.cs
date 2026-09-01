using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ILocationService
    {
        Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default);
        Task UpdateAsync(Location location, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}