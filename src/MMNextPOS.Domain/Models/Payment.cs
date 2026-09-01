namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Payment receipt (cash, bank, cheque, card).
    ///</summary>
    public class Payment : EntityBase
    {
        public string PaymentNo { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "Customer"; // Customer, Supplier
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public int? SaleId { get; set; }
        public int? PurchaseId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = "Cash"; // Cash, Bank, Cheque, Card, Mobile
        public string? ReferenceNo { get; set; }
        public string? BankName { get; set; }
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string Status { get; set; } = "Cleared"; // Pending, Cleared, Bounced, Cancelled
        public int? ReceivedByUserId { get; set; }
        public string? Notes { get; set; }
    }
}
