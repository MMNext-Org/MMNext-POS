using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Starman - Stock balance report DTO.
    /// </summary>
    public class StarStockBalanceReport : EntityBase
    {
        public int LocationId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastMovementDate { get; set; }
    }
}
