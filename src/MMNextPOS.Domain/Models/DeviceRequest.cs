using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Device registration request for license management.
    /// </summary>
    public class DeviceRequest : EntityBase
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceFingerprint { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string CpuId { get; set; } = string.Empty;
        public string HardDiskSerial { get; set; } = string.Empty;
        public string OsVersion { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string RequestType { get; set; } = string.Empty; // New, Renew, Transfer
        public int? ApprovedByUserId { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
