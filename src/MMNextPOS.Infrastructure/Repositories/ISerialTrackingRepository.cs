using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISerialTrackingRepository : IRepository<SerialTracking>
    {
        Task<IReadOnlyList<SerialTracking>> GetBySerialNumberIdAsync(int serialNumberId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialTracking>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialTracking>> GetByLocationAsync(int locationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialTracking>> GetByMovementTypeAsync(SerialMovementType movementType, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialTracking>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    }

    public class SerialTrackingRepository : GenericRepository<SerialTracking>, ISerialTrackingRepository
    {
        public SerialTrackingRepository(IUnitOfWork unitOfWork) : base(unitOfWork, "SerialTrackings")
        {
        }

        public async Task<IReadOnlyList<SerialTracking>> GetBySerialNumberIdAsync(int serialNumberId, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(st => st.SerialNumberId == serialNumberId).OrderByDescending(st => st.Timestamp).ToList();
        }

        public async Task<IReadOnlyList<SerialTracking>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(st => st.SerialNumber != null && st.SerialNumber.ProductId == productId).ToList();
        }

        public async Task<IReadOnlyList<SerialTracking>> GetByLocationAsync(int locationId, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(st => st.FromLocationId == locationId || st.ToLocationId == locationId).ToList();
        }

        public async Task<IReadOnlyList<SerialTracking>> GetByMovementTypeAsync(SerialMovementType movementType, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(st => st.MovementType == movementType).ToList();
        }

        public async Task<IReadOnlyList<SerialTracking>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(st => st.Timestamp >= from && st.Timestamp <= to).OrderByDescending(st => st.Timestamp).ToList();
        }
    }
}
