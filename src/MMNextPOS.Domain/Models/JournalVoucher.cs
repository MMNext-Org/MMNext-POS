namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Journal voucher header — one double-entry accounting record.
    /// Status: Draft (editable), Posted (locked), Voided (cancelled).
    /// CreatedBy / UpdatedBy / IsDeleted / UpdatedAt are inherited from EntityBase.
    /// </summary>
    public class JournalVoucher : EntityBase
    {
        public string VoucherNo { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public string? ReferenceNo { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Posted, Voided
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Line items of this voucher.
        /// </summary>
        public ICollection<JournalVoucherDetail> Details { get; set; } = new List<JournalVoucherDetail>();
    }
}
