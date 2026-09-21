using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Represents a batch/lot of products with manufacturing and expiry information.
    /// Used for perishable items (food, pharma, cosmetics), chemicals, and lot-tracked goods.
    /// </summary>
    public class SerialBatch : EntityBase
    {
        /// <summary>
        /// The batch/lot number identifier.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        /// <summary>
        /// Product this batch contains.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Manufacture date of this batch.
        /// </summary>
        public DateTime ManufactureDate { get; set; }

        /// <summary>
        /// Expiry date of this batch (for perishables).
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Initial quantity in this batch.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Remaining quantity in stock.
        /// </summary>
        public int RemainingQuantity { get; set; }

        /// <summary>
        /// Supplier who provided this batch.
        /// </summary>
        public int? SupplierId { get; set; }

        /// <summary>
        /// Cost per unit for this batch.
        /// </summary>
        public decimal CostPerUnit { get; set; }

        /// <summary>
        /// Storage location for this batch.
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Whether this batch is active (not soft-deleted).
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Checks if this batch is expired.
        /// </summary>
        public bool IsExpired => ExpiryDate < DateTime.UtcNow;

        /// <summary>
        /// Checks if this batch is expiring soon (within specified days).
        /// </summary>
        public bool IsExpiringSoon(int daysThreshold = 30)
        {
            return !IsExpired && ExpiryDate <= DateTime.UtcNow.AddDays(daysThreshold);
        }

        /// <summary>
        /// Days until expiry (negative if expired).
        /// </summary>
        public int DaysUntilExpiry => (int)(ExpiryDate - DateTime.UtcNow).TotalDays;
    }
}
