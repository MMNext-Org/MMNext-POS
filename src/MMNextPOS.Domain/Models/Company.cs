namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Company / business entity for multi-company support.
    /// </summary>
    public class Company : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public string? TaxId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? LogoPath { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
