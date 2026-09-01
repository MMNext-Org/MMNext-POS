namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Line items of a stock movement (product, quantity, cost).
    ///</summary>
    public class StockMovementDetail : EntityBase
    {
        public int StockMovementId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
        public string? SerialNumber { get; set; } // For serial-tracked items
        public string? Notes { get; set; }
    }
}
