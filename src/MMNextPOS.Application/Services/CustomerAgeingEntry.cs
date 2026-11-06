namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Accounts-receivable ageing summary for a single customer, bucketed by
    /// days outstanding since the transaction date.
    /// </summary>
    public class CustomerAgeingEntry
    {
        public int CustomerId { get; set; }

        /// <summary>Balance outstanding for 0-30 days.</summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>Balance outstanding for 31-60 days.</summary>
        public decimal Days31To60 { get; set; }

        /// <summary>Balance outstanding for 61-90 days.</summary>
        public decimal Days61To90 { get; set; }

        /// <summary>Balance outstanding for more than 90 days.</summary>
        public decimal Days90Plus { get; set; }

        /// <summary>Total open balance across all buckets.</summary>
        public decimal TotalBalance => CurrentBalance + Days31To60 + Days61To90 + Days90Plus;
    }
}
