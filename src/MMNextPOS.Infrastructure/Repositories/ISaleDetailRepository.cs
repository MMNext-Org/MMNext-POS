using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISaleDetailRepository : IRepository<SaleDetail>
    {
        Task<IReadOnlyList<SaleDetail>> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
    }
}
