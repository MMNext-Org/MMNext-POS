namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Application user (login credentials and profile).
    /// </summary>
    public class User : EntityBase
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public int? LocationId { get; set; } // Primary location
        public int? CompanyId { get; set; } // Primary company
    }
}
