using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ILicenseInfoService
    {
        Task<LicenseInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<LicenseInfo?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LicenseInfo>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<LicenseInfo> AddAsync(LicenseInfo entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(LicenseInfo entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
