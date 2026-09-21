using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Generic stock movement header (issue, receive, damaged, lost, adjust, transfer, assembly, etc.).
    /// </summary>
    public class StockMovement : EntityBase
    {
        [Required]
        [MaxLength(50)]
        public string MovementNo { get; set; } = string.Empty;

        /// <summary>
        /// Type of stock movement. Using string for DB compatibility with enum backing.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string MovementType { get; set; } = StockMovementType.Adjust.ToDbString();

        /// <summary>
        /// Reason code for the movement (e.g., "DAMAGED", "EXPIRED", "THEFT", "CYCLE_COUNT").
        /// </summary>
        [MaxLength(50)]
        public string? ReasonCode { get; set; }

        /// <summary>
        /// Human-readable reason/description.
        /// </summary>
        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime MovementDate { get; set; } = DateTime.UtcNow;

        public int? LocationId { get; set; }

        /// <summary>
        /// Source location for transfers (TransferOut).
        /// </summary>
        public int? FromLocationId { get; set; }

        /// <summary>
        /// Destination location for transfers (TransferIn).
        /// </summary>
        public int? ToLocationId { get; set; }

        public int? SupplierId { get; set; } // For receive/purchase movements
        public int? CustomerId { get; set; } // For issue/sale movements
        public int? ProductId { get; set; } // For single-product movements (legacy/simple)

        /// <summary>
        /// Reference to source document (SaleId, PurchaseId, TransferId, AssemblyId, etc.)
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// Type of reference document (Sale, Purchase, Transfer, Assembly, etc.)
        /// </summary>
        [MaxLength(20)]
        public string? ReferenceType { get; set; }

        public int Quantity { get; set; }
        public string Status { get; set; } = "Active"; // Active, Cancelled, Reversed
        public int? CreatedByUserId { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<StockMovementDetail>? Details { get; set; }

        /// <summary>
        /// Gets the movement type as enum. Computed from MovementType — not a DB column.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public StockMovementType MovementTypeEnum
        {
            get => StockMovementTypeExtensions.FromDbString(MovementType);
            set => MovementType = value.ToDbString();
        }
    }
}
