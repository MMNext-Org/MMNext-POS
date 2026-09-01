namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Discount master (predefined discount levels/rules).
    /// </summary>
    public class Discount : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; } // e.g., 0.10 for 10%
        public decimal? MinimumAmount { get; set; } // Minimum purchase to apply
        public decimal? MaximumAmount { get; set; } // Maximum discount cap
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
