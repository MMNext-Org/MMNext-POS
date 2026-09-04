using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Starman - Cash flow report DTO.
    /// </summary>
    public class StarCashFlowReport : EntityBase
    {
        public int LocationId { get; set; }
        public DateTime ReportDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalCollections { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
