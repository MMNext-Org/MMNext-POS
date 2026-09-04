using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Client update request for auto-update mechanism.
    /// </summary>
    public class ClientUpdateRequest : EntityBase
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // PC, Mobile
        public string CurrentVersion { get; set; } = string.Empty;
        public string RequestedVersion { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Delivered
        public int? ApprovedByUserId { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
