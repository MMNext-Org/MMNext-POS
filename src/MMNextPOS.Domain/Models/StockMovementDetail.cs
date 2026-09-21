using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Line items of a stock movement (product, quantity, cost, serial/batch tracking).
    /// </summary>
    public class StockMovementDetail : EntityBase
    {
        public int StockMovementId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Serial number for serial-tracked items.
        /// </summary>
        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Batch/lot number for batch-tracked items.
        /// </summary>
        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Expiry date for batch-tracked perishable items.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Location this detail line applies to (for multi-location movements).
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Reference to source document detail (SaleDetailId, PurchaseDetailId, etc.)
        /// </summary>
        public int? ReferenceDetailId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public StockMovement? StockMovement { get; set; }
        public Product? Product { get; set; }
    }
}
