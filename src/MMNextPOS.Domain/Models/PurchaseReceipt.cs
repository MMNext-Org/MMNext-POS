using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Purchase receipt / voucher for printing
    /// </summary>
    public class PurchaseReceipt : EntityBase
    {
        [Required]
        [MaxLength(50)]
        public string ReceiptNo { get; set; } = string.Empty;

        public int PurchaseId { get; set; }

        [MaxLength(150)]
        public string? SupplierName { get; set; }

        [MaxLength(20)]
        public string? SupplierPhone { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int? PrintedByUserId { get; set; }
        public DateTime? PrintedAt { get; set; }

        public int PrintCount { get; set; } = 0;

        [MaxLength(20)]
        public string? TerminalId { get; set; }

        public bool IsReprint { get; set; }

        [MaxLength(500)]
        public string? FooterText { get; set; }

        // Navigation
        public virtual Purchase? Purchase { get; set; }
        public virtual ICollection<PurchaseReceiptDetail> Details { get; set; } = new List<PurchaseReceiptDetail>();
    }

    /// <summary>
    /// Purchase receipt detail line items
    /// </summary>
    public class PurchaseReceiptDetail : EntityBase
    {
        public int PurchaseReceiptId { get; set; }

        [MaxLength(50)]
        public string? ProductSku { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public int ReceivedQuantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }

        [MaxLength(50)]
        public string? UnitOfMeasure { get; set; }

        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        public DateTime? ExpiryDate { get; set; }

        // Navigation
        public virtual PurchaseReceipt? PurchaseReceipt { get; set; }
    }
}