namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Sales return transaction (customer returns goods).
    /// </summary>
    public class SalesReturn : EntityBase
    {
        public string ReturnNo { get; set; } = string.Empty;
        public int SaleId { get; set; }
        public int CustomerId { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed, Voided
        public int? CreatedByUserId { get; set; }
    }
}
