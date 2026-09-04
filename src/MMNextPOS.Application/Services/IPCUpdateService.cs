using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IPCUpdateService
    {
        Task<PCUpdate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PCUpdate>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PCUpdate> AddAsync(PCUpdate entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(PCUpdate entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
