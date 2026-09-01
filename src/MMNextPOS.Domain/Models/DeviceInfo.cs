namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Device binding record (one license per device fingerprint).
    ///</summary>
    public class DeviceInfo : EntityBase
    {
        public int RegistrationId { get; set; }
        public string DeviceFingerprint { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string? MacAddress { get; set; }
        public string? CpuId { get; set; }
        public string? HardDiskSerial { get; set; }
        public string? OsVersion { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
