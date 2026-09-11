using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents a sales transaction (header) with customer info for display.
    /// </summary>
    public class Sale : EntityBase
    {
        [Key]
        public new int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        // Not mapped to DB column; populated via JOIN or lookup for display
        [NotMapped]
        public string? CustomerName { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Sale status (e.g., "Completed", "Hold", "Voided", "Draft")
        /// </summary>
        [MaxLength(50)]
        public string? Status { get; set; }

        /// <summary>
        /// Optional location/branch identifier for multi-location support
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Auto-generated invoice number (e.g., "INV-2026-000042")
        /// </summary>
        [MaxLength(50)]
        public string? InvoiceNo { get; set; }

        /// <summary>
        /// Navigation property for sale line items
        /// </summary>
        public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}
