using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IMobileClientService
    {
        Task<MobileClient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MobileClient>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MobileClient> AddAsync(MobileClient entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(MobileClient entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
