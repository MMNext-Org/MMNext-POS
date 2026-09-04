using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IStarStockTransferReceivedService
    {
        Task<StarStockTransferReceived?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StarStockTransferReceived>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<StarStockTransferReceived> AddAsync(StarStockTransferReceived entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(StarStockTransferReceived entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
