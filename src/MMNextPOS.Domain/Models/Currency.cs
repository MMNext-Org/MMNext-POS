namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Currency definition for multi-currency support.
    /// </summary>
    public class Currency : EntityBase
    {
        public string Code { get; set; } = string.Empty; // e.g., USD, EUR, MMK
        public string Name { get; set; } = string.Empty;
        public string? Symbol { get; set; } // e.g., $, €, K
        public decimal ExchangeRate { get; set; } = 1.0m; // Relative to base currency
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; }
    }
}
