namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Software license registration.
    ///</summary>
    public class Registration : EntityBase
    {
        public string LicenseKey { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxUsers { get; set; } = 1;
        public int MaxDevices { get; set; } = 1;
        public string Status { get; set; } = "Active"; // Active, Expired, Suspended, Cancelled
        public string? Notes { get; set; }
    }
}
