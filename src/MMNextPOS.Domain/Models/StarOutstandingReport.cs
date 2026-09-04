using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Starman - Outstanding report DTO (multi-site).
    /// </summary>
    public class StarOutstandingReport : EntityBase
    {
        public int LocationId { get; set; }
        public string PartyType { get; set; } = string.Empty; // Customer, Supplier
        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal ClosingBalance { get; set; }
        public DateTime AsOfDate { get; set; }
    }
}
