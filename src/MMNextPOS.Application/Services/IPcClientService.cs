using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IPcClientService
    {
        Task<PcClient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PcClient>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PcClient> AddAsync(PcClient entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(PcClient entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
