namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Supplier outstanding balance (accounts payable).
    ///</summary>
    public class SupplierOutstanding : EntityBase
    {
        public int SupplierId { get; set; }
        public int? PurchaseId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal DebitAmount { get; set; } // Amount paid
        public decimal CreditAmount { get; set; } // Amount owed
        public decimal Balance { get; set; } // Running balance
        public string? Description { get; set; }
        public string Status { get; set; } = "Open";
    }
}
