namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Line items of a purchase order.
    /// </summary>
    public class PurchaseDetail : EntityBase
    {
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }
}
