using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Starman - Issue header for stock issues between locations.
    /// </summary>
    public class IssueHeader : EntityBase
    {
        public string IssueNo { get; set; } = string.Empty;
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, InTransit, Received, Cancelled
        public string IssueType { get; set; } = string.Empty; // Transfer, Return, Adjustment
        public string Reason { get; set; } = string.Empty;
        public int? IssuedByUserId { get; set; }
        public int? ReceivedByUserId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
