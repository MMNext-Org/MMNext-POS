using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IDeviceInfoRepository : IRepository<DeviceInfo>
    {
        Task<DeviceInfo?> GetByFingerprintAsync(string fingerprint, CancellationToken cancellationToken = default);
        Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    }
}
