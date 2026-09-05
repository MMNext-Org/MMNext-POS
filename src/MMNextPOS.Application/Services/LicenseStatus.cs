using System;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Reason a license is considered invalid. Maps 1:1 to user-facing copy
    /// the LicenseRegistrationForm shows in its status banner.
    /// </summary>
    public enum LicenseInvalidReason
    {
        Valid = 0,
        NotRegistered,
        Inactive,
        Suspended,
        Cancelled,
        Expired,
        DeviceNotBound,
        DeviceLimitReached,
        Unknown
    }

    /// <summary>
    /// Result of <see cref="ILicenseGuard.CheckAsync"/>. <see cref="IsValid"/>
    /// is the only field the host cares about; the rest is exposed for UI
    /// status messages and logging.
    /// </summary>
    public sealed class LicenseStatus
    {
        public bool IsValid { get; init; }
        public LicenseInvalidReason Reason { get; init; } = LicenseInvalidReason.Valid;
        public string Message { get; init; } = string.Empty;
        public LicenseInfo? License { get; init; }
        public DeviceInfo? Device { get; init; }
        public DeviceFingerprint? Fingerprint { get; init; }
        public DateTime? CheckedAtUtc { get; init; } = DateTime.UtcNow;

        public static LicenseStatus Ok(LicenseInfo license, DeviceInfo? device, DeviceFingerprint fingerprint) =>
            new()
            {
                IsValid = true,
                Reason = LicenseInvalidReason.Valid,
                Message = "License is valid.",
                License = license,
                Device = device,
                Fingerprint = fingerprint
            };

        public static LicenseStatus Invalid(LicenseInvalidReason reason, string message, LicenseInfo? license = null, DeviceFingerprint? fingerprint = null) =>
            new()
            {
                IsValid = false,
                Reason = reason,
                Message = message,
                License = license,
                Fingerprint = fingerprint
            };
    }
}
