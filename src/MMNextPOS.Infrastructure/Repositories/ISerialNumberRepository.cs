using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISerialNumberRepository : IRepository<SerialNumber>
    {
        Task<SerialNumber?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialNumber>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialNumber>> GetAvailableByProductIdAsync(int productId, int? locationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialNumber>> GetByLocationIdAsync(int locationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialNumber>> GetByStatusAsync(SerialStatus status, CancellationToken cancellationToken = default);
        Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken cancellationToken = default);
    }
}
