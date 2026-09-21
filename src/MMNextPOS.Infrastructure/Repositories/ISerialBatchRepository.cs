using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISerialBatchRepository : IRepository<SerialBatch>
    {
        Task<SerialBatch?> GetByBatchNumberAsync(string batchNumber, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialBatch>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialBatch>> GetExpiringBatchesAsync(int daysThreshold = 30, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialBatch>> GetExpiredBatchesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialBatch>> GetActiveBatchesByProductAsync(int productId, CancellationToken cancellationToken = default);
    }

    public class SerialBatchRepository : GenericRepository<SerialBatch>, ISerialBatchRepository
    {
        public SerialBatchRepository(IUnitOfWork unitOfWork) : base(unitOfWork, "SerialBatches")
        {
        }

        public async Task<SerialBatch?> GetByBatchNumberAsync(string batchNumber, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.FirstOrDefault(b => b.BatchNumber == batchNumber && b.IsActive);
        }

        public async Task<IReadOnlyList<SerialBatch>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(b => b.ProductId == productId && b.IsActive).ToList();
        }

        public async Task<IReadOnlyList<SerialBatch>> GetExpiringBatchesAsync(int daysThreshold = 30, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            var cutoff = DateTime.UtcNow.AddDays(daysThreshold);
            return all.Where(b => b.IsActive && !b.IsExpired && b.ExpiryDate <= cutoff)
                .OrderBy(b => b.ExpiryDate)
                .ToList();
        }

        public async Task<IReadOnlyList<SerialBatch>> GetExpiredBatchesAsync(CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(b => b.IsActive && b.IsExpired)
                .OrderBy(b => b.ExpiryDate)
                .ToList();
        }

        public async Task<IReadOnlyList<SerialBatch>> GetActiveBatchesByProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(b => b.ProductId == productId && b.IsActive && !b.IsExpired && b.RemainingQuantity > 0)
                .OrderBy(b => b.ExpiryDate) // FEFO: First Expired, First Out
                .ToList();
        }
    }
}
