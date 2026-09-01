namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Expense transaction (rent, utilities, salary, supplies, etc.).
    ///</summary>
    public class Expense : EntityBase
    {
        public string ExpenseNo { get; set; } = string.Empty;
        public int ExpenseTypeId { get; set; }
        public DateTime ExpenseDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? ReferenceNo { get; set; }
        public int? LocationId { get; set; }
        public int? VendorId { get; set; } // Optional supplier link
        public string? Description { get; set; }
        public string? ReceiptPath { get; set; }
        public string Status { get; set; } = "Active";
        public int? CreatedByUserId { get; set; }
    }
}
