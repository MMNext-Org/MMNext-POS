using System.Threading;
using System.Threading.Tasks;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Enforces license + device-binding rules at application start. The
    /// returned <see cref="LicenseStatus"/> aggregates a pass/fail decision
    /// with a human-readable reason.
    /// </summary>
    public interface ILicenseGuard
    {
        Task<LicenseStatus> CheckAsync(CancellationToken cancellationToken = default);
    }
}
