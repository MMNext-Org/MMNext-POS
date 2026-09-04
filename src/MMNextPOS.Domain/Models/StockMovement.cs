namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Generic stock movement header (issue, receive, damaged, lost, adjust).
    ///</summary>
    public class StockMovement : EntityBase
    {
        public string MovementNo { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty; // Opening, Issue, Receive, Damaged, Lost, Adjust
        public DateTime MovementDate { get; set; }
        public int? LocationId { get; set; }
        public int? SupplierId { get; set; } // For receive movements
        public int? CustomerId { get; set; } // For issue movements
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Active";
        public int? CreatedByUserId { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}