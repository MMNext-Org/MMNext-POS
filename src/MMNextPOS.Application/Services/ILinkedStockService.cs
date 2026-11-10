using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public interface ILinkedStockService
    {
        Task<LinkedStock?> GetLinkedStockByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LinkedStock>> GetLinkedStocksAsync(int? productId = null, string? linkType = null, CancellationToken cancellationToken = default);
        Task<PagedResult<LinkedStock>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<LinkedStock> AddLinkedStockAsync(LinkedStock linkedStock, CancellationToken cancellationToken = default);
        Task UpdateLinkedStockAsync(LinkedStock linkedStock, CancellationToken cancellationToken = default);
        Task DeleteLinkedStockAsync(int id, CancellationToken cancellationToken = default);
    }
}