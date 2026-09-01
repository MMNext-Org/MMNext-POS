namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Line items of a temporary sale (SaleTemp).
    /// </summary>
    public class SaleTempDetail : EntityBase
    {
        public int SaleTempId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }
}
