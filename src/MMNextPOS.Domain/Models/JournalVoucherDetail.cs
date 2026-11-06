namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Journal voucher line item — one debit or credit posting to an account.
    /// CreatedBy / UpdatedBy / IsDeleted / UpdatedAt are inherited from EntityBase.
    /// </summary>
    public class JournalVoucherDetail : EntityBase
    {
        public int JournalVoucherId { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string? Description { get; set; }
        public int LineNo { get; set; }
    }
}
