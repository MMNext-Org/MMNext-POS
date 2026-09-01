namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Supplier master data (vendors, wholesalers).
    /// </summary>
    public class Supplier : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ContactPerson { get; set; }
        public string? TaxId { get; set; }
        public decimal? CreditLimit { get; set; }
        public int PaymentTermDays { get; set; } = 0; // Days for payment
        public bool IsActive { get; set; } = true;
    }
}
