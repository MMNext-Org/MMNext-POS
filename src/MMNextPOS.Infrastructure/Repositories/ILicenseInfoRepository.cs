using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ILicenseInfoRepository : IRepository<LicenseInfo>
    {
        Task<LicenseInfo?> GetCurrentAsync(CancellationToken cancellationToken = default);
        Task<LicenseInfo?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default);
    }
}
