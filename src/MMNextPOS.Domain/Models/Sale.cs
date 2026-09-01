using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents a sales transaction (header) with customer info for display.
    /// </summary>
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        // Not mapped to DB column; populated via JOIN or lookup for display
        public string? CustomerName { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
    }
}