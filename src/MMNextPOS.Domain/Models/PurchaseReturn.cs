namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Purchase return header (goods returned to supplier).
    /// </summary>
    public class PurchaseReturn : EntityBase
    {
        public string ReturnNo { get; set; } = string.Empty;
        public int PurchaseId { get; set; }
        public int SupplierId { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Active"; // Active, Cancelled
        public int? CreatedByUserId { get; set; }
        public string? Notes { get; set; }
    }
}
