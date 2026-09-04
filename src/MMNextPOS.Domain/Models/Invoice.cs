using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents an invoice generated from a sale.
    /// </summary>
    public class Invoice : EntityBase
    {
        public string InvoiceNo { get; set; } = string.Empty;
        public int SaleId { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public decimal AmountDue { get; set; }
        public string Status { get; set; } = "Active"; // Active, Paid, Voided
        public int? CreatedByUserId { get; set; }
    }
}
