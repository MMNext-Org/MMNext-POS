using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IDeviceRequestService
    {
        Task<DeviceRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DeviceRequest>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DeviceRequest> AddAsync(DeviceRequest entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(DeviceRequest entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
