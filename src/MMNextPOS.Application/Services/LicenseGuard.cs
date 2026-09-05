using System;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Evaluates the active license + device binding and returns a single
    /// pass/fail decision. The guard never throws for "expected" invalid
    /// states (no license, expired, etc.); only infrastructure failures
    /// (DB unreachable, etc.) propagate.
    /// </summary>
    public sealed class LicenseGuard : ILicenseGuard
    {
        private readonly ILicenseInfoRepository _licenseRepo;
        private readonly IDeviceInfoRepository _deviceRepo;
        private readonly IDeviceFingerprintService _fingerprintService;
        private readonly IAuditService _auditService;

        public LicenseGuard(
            ILicenseInfoRepository licenseRepo,
            IDeviceInfoRepository deviceRepo,
            IDeviceFingerprintService fingerprintService,
            IAuditService auditService)
        {
            _licenseRepo = licenseRepo ?? throw new ArgumentNullException(nameof(licenseRepo));
            _deviceRepo = deviceRepo ?? throw new ArgumentNullException(nameof(deviceRepo));
            _fingerprintService = fingerprintService ?? throw new ArgumentNullException(nameof(fingerprintService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<LicenseStatus> CheckAsync(CancellationToken cancellationToken = default)
        {
            var fingerprint = _fingerprintService.GetCurrent();
            var license = await _licenseRepo.GetCurrentAsync(cancellationToken).ConfigureAwait(false);

            if (license == null)
            {
                return LicenseStatus.Invalid(
                    LicenseInvalidReason.NotRegistered,
                    "No license is registered for this installation.",
                    fingerprint: fingerprint);
            }

            if (!string.Equals(license.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                var reason = license.Status?.ToLowerInvariant() switch
                {
                    "suspended" => LicenseInvalidReason.Suspended,
                    "cancelled" => LicenseInvalidReason.Cancelled,
                    _ => LicenseInvalidReason.Inactive
                };
                return LicenseStatus.Invalid(
                    reason,
                    $"License status is '{license.Status}'. Registration or renewal is required.",
                    license: license,
                    fingerprint: fingerprint);
            }

            if (license.ExpiryDate <= DateTime.UtcNow)
            {
                return LicenseStatus.Invalid(
                    LicenseInvalidReason.Expired,
                    $"License expired on {license.ExpiryDate:yyyy-MM-dd}. Please renew.",
                    license: license,
                    fingerprint: fingerprint);
            }

            var device = await _deviceRepo.GetByFingerprintAsync(fingerprint.Hash, cancellationToken).ConfigureAwait(false);
            if (device == null || !device.IsActive)
            {
                return LicenseStatus.Invalid(
                    LicenseInvalidReason.DeviceNotBound,
                    "This device is not authorised to run the application. Please register it.",
                    license: license,
                    fingerprint: fingerprint);
            }

            // Heartbeat: update LastSeenAt without blocking the caller on a write.
            _ = UpdateLastSeenAsync(device, cancellationToken);

            return LicenseStatus.Ok(license, device, fingerprint);
        }

        private async Task UpdateLastSeenAsync(DeviceInfo device, CancellationToken cancellationToken)
        {
            try
            {
                device.LastSeenAt = DateTime.UtcNow;
                await _deviceRepo.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Telemetry best-effort; never let a heartbeat write block startup.
            }
        }
    }
}
