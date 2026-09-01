namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Historical sale price records (price changes over time).
    ///</summary>
    public class SalePriceHistory : EntityBase
    {
        public int ProductId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime EffectiveDate { get; set; }
        public int? ChangedByUserId { get; set; }
        public string? Reason { get; set; }
    }
}
