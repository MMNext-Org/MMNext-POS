using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents a line item of a sale.
    /// </summary>
    public class SaleDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal LineTotal { get; set; }

        /// <summary>
        /// Navigation property for the product
        /// </summary>
        public Product? Product { get; set; }

        /// <summary>
        /// Calculates the line total: Quantity * UnitPrice - DiscountAmount + TaxAmount
        /// </summary>
        public decimal CalculateLineTotal()
        {
            return Quantity * UnitPrice - DiscountAmount + TaxAmount;
        }
    }
}
