using System;
using System.ComponentModel.DataAnnotations;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Payment voucher for cash/bank receipts
    /// </summary>
    public class PaymentVoucher : EntityBase
    {
        [Required]
        [MaxLength(50)]
        public string VoucherNo { get; set; } = string.Empty;

        [Required]
        public string PaymentType { get; set; } = "Customer"; // Customer, Supplier

        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }

        public int? SaleId { get; set; }
        public int? PurchaseId { get; set; }
        public int? OutstandingId { get; set; }

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Method { get; set; } = "Cash"; // Cash, Bank, Cheque, Card, Mobile

        [MaxLength(50)]
        public string? ReferenceNo { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? ChequeNo { get; set; }

        public DateTime? ChequeDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Cleared"; // Pending, Cleared, Bounced, Cancelled

        public int? ReceivedByUserId { get; set; }
        public DateTime? ReceivedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(20)]
        public string? TerminalId { get; set; }
    }
}