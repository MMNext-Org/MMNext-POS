namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Stable description of a device used for license binding.
    /// </summary>
    public sealed record DeviceFingerprint(
        string Hash,
        string MachineName,
        string MacAddress,
        string? CpuId,
        string? HardDiskSerial,
        string OsVersion);

    /// <summary>
    /// Computes a stable device fingerprint and exposes the raw components.
    /// </summary>
    public interface IDeviceFingerprintService
    {
        /// <summary>
        /// Returns the device fingerprint for the current machine. The hash is
        /// deterministic for the same machine (MAC + hostname + OS).
        /// </summary>
        DeviceFingerprint GetCurrent();
    }
}
