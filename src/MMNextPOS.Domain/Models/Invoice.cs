using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents an invoice generated from a sale.
    /// </summary>
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal AmountDue { get; set; }
    }
}
