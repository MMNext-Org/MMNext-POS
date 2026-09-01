namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Temporary sales used for live-editing and drafting before finalization.
    /// </summary>
    public class SaleTemp : EntityBase
    {
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Finalized, Voided
        public int? CreatedByUserId { get; set; }
        public string? Notes { get; set; }
        public int? LocationId { get; set; }
    }
}
