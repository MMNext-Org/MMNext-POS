namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Purchase order header (goods received from supplier).
    /// </summary>
    public class Purchase : EntityBase
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = "Active"; // Active, Hold, Returned, Cancelled
        public int? LocationId { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? Notes { get; set; }
    }
}
