namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Inter-location stock transfer header (Starman multi-site).
    ///</summary>
    public class StockTransfer : EntityBase
    {
        public string TransferNo { get; set; } = string.Empty;
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public DateTime TransferDate { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, InTransit, PartiallyReceived, Received, Cancelled
        public string? Notes { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? ReleasedByUserId { get; set; }
        public int? ReceivedByUserId { get; set; }
        public int? CancelledByUserId { get; set; }
        public string? CancelReason { get; set; }
    }
}
