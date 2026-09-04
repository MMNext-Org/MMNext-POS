using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Sale receipt / voucher for printing
    /// </summary>
    public class SaleReceipt : EntityBase
    {
        [Required]
        [MaxLength(50)]
        public string ReceiptNo { get; set; } = string.Empty;

        public int SaleId { get; set; }

        [MaxLength(150)]
        public string? CustomerName { get; set; }

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // Cash, Card, Mobile, Cheque

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
        public virtual Sale? Sale { get; set; }
        public virtual ICollection<SaleReceiptDetail> Details { get; set; } = new List<SaleReceiptDetail>();
    }

    /// <summary>
    /// Sale receipt detail line items
    /// </summary>
    public class SaleReceiptDetail : EntityBase
    {
        public int SaleReceiptId { get; set; }

        [MaxLength(50)]
        public string? ProductSku { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal LineTotal { get; set; }

        [MaxLength(50)]
        public string? UnitOfMeasure { get; set; }

        // Navigation
        public virtual SaleReceipt? SaleReceipt { get; set; }
    }
}