using System;

namespace MMNextPOS.Domain.Models
{
    /// <summary>
    /// Starman - Profit & Loss report DTO.
    /// </summary>
    public class StarProfitLossReport : EntityBase
    {
        public int LocationId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
