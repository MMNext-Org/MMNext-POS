namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Line items of a stock transfer.
    ///</summary>
    public class StockTransferDetail : EntityBase
    {
        public int StockTransferId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? ReceivedQuantity { get; set; }
        public string? SerialNumber { get; set; }
        public string? Notes { get; set; }
    }
}
