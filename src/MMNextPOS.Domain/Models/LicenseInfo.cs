using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// License information for the application.
    /// </summary>
    public class LicenseInfo : EntityBase
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxUsers { get; set; } = 1;
        public int MaxDevices { get; set; } = 1;
        public string Status { get; set; } = "Active"; // Active, Expired, Suspended, Cancelled
        public string Notes { get; set; } = string.Empty;
        public bool IsActivated { get; set; } = false;
        public DateTime? ActivatedDate { get; set; }
        public string ActivatedDeviceId { get; set; } = string.Empty;
    }
}
