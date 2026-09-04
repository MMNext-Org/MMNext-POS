using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Mobile Client information for license management.
    /// </summary>
    public class MobileClient : EntityBase
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string DeviceModel { get; set; } = string.Empty;
        public string OsVersion { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public DateTime LastSyncDate { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public string Status { get; set; } = "Active"; // Active, Inactive, Blocked
        public string PushToken { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
