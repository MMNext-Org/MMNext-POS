using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents a product (stock item) in the POS system.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique stock keeping unit.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Sku { get; set; } = string.Empty;

        /// <summary>
        /// Human‑readable name.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Unit selling price.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        /// <summary>
        /// Available quantity in stock.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Soft delete flag. Products are never permanently deleted;
        /// setting IsActive to false preserves the record for historical sales.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Minimum stock alert level. When stock falls below this value,
        /// the product is considered low stock and alerts are triggered.
        /// </summary>
        public int? MinStockAlertLevel { get; set; }
    }
}
