namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Customer outstanding balance (accounts receivable).
    ///</summary>
    public class CustomerOutstanding : EntityBase
    {
        public int CustomerId { get; set; }
        public int? SaleId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal DebitAmount { get; set; } // Amount owed
        public decimal CreditAmount { get; set; } // Amount paid
        public decimal Balance { get; set; } // Running balance
        public string? Description { get; set; }
        public string Status { get; set; } = "Open"; // Open, Closed
    }
}
