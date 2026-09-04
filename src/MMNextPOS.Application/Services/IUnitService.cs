using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IUnitService
    {
        Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Unit>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Unit> AddAsync(Unit unit, CancellationToken cancellationToken = default);
        Task UpdateAsync(Unit unit, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
