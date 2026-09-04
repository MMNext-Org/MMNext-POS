using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public interface ISalesReturnService
    {
        Task<SalesReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SalesReturn>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<SalesReturn>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<SalesReturn> AddAsync(SalesReturn salesReturn, CancellationToken cancellationToken = default);
        Task UpdateAsync(SalesReturn salesReturn, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
