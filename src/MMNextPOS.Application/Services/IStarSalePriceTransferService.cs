using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IStarSalePriceTransferService
    {
        Task<StarSalePriceTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StarSalePriceTransfer>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<StarSalePriceTransfer> AddAsync(StarSalePriceTransfer entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(StarSalePriceTransfer entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
