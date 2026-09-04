using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Starman - Sale price transfer/acceptance between locations.
    /// </summary>
    public class StarSalePriceTransfer : EntityBase
    {
        public string TransferNo { get; set; } = string.Empty;
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public DateTime TransferDate { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
        public int? AcceptedByUserId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
