using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Starman - Multi-site stock transfer received/accepted.
    /// </summary>
    public class StarStockTransferReceived : EntityBase
    {
        public string TransferNo { get; set; } = string.Empty;
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public DateTime TransferDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
        public int? ReceivedByUserId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
