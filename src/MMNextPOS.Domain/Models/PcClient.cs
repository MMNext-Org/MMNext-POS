using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// PC Client information for license management.
    /// </summary>
    public class PcClient : EntityBase
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string CpuId { get; set; } = string.Empty;
        public string HardDiskSerial { get; set; } = string.Empty;
        public string OsVersion { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public DateTime LastSyncDate { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public string Status { get; set; } = "Active"; // Active, Inactive, Blocked
        public string IpAddress { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
