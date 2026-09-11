using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISalesReturnDetailRepository : IRepository<SalesReturnDetail>
    {
        /// <summary>
        /// Gets all return details for a specific sale detail.
        /// </summary>
        Task<IReadOnlyList<SalesReturnDetail>> GetBySaleDetailIdAsync(int saleDetailId, CancellationToken cancellationToken = default);
    }
}
