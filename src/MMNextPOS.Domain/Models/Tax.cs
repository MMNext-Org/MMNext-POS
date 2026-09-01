namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Tax configuration (e.g., 5% VAT, 10% Service Tax).
    /// </summary>
    public class Tax : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; } // e.g., 0.05 for 5%
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
