using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SerialNumberRepository : GenericRepository<SerialNumber>, ISerialNumberRepository
    {
        public SerialNumberRepository(IUnitOfWork unitOfWork) : base(unitOfWork, "SerialNumbers")
        {
        }

        public async Task<SerialNumber?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.FirstOrDefault(s => s.SerialNumberValue == serialNumber && s.IsActive);
        }

        public async Task<IReadOnlyList<SerialNumber>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(s => s.ProductId == productId && s.IsActive).ToList();
        }

        public async Task<IReadOnlyList<SerialNumber>> GetAvailableByProductIdAsync(int productId, int? locationId, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            var filtered = all.Where(s => s.ProductId == productId && s.IsActive && s.Status == SerialStatus.Available);
            if (locationId.HasValue)
            {
                filtered = filtered.Where(s => s.LocationId == locationId.Value);
            }
            return filtered.ToList();
        }

        public async Task<IReadOnlyList<SerialNumber>> GetByLocationIdAsync(int locationId, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(s => s.LocationId == locationId && s.IsActive).ToList();
        }

        public async Task<IReadOnlyList<SerialNumber>> GetByStatusAsync(SerialStatus status, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Where(s => s.Status == status && s.IsActive).ToList();
        }

        public async Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken cancellationToken = default)
        {
            var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.Any(s => s.SerialNumberValue == serialNumber && s.IsActive);
        }
    }
}
