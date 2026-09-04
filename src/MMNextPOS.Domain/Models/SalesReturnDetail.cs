namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Detail line for a sales return.
    /// </summary>
    public class SalesReturnDetail : EntityBase
    {
        public int SalesReturnId { get; set; }
        public int SaleDetailId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Reason { get; set; }
    }
}
