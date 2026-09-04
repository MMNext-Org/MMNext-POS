namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Line items of a purchase return.
    /// </summary>
    public class PurchaseReturnDetail : EntityBase
    {
        public int PurchaseReturnId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }
}