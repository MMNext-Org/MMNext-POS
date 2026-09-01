using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISaleRepository
    {
        Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);
        Task<Sale> CreateSaleWithDetailsAsync(Sale sale, IEnumerable<SaleDetail> details, CancellationToken cancellationToken = default);
        Task AddDetailAsync(SaleDetail detail, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Sale>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default);
    }
}
