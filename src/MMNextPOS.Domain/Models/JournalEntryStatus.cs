namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Status of a journal voucher. Matches the string values stored in JournalVoucher.Status.
    /// </summary>
    public enum JournalEntryStatus
    {
        Draft = 0,
        Posted = 1,
        Voided = 2
    }
}
