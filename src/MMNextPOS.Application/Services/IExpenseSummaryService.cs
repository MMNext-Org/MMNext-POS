using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IExpenseSummaryService
    {
        Task<MonthlyExpenseSummary> GetMonthlySummaryAsync(int year, int month, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MonthlyExpenseSummary>> GetYearlySummaryAsync(int year, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ExpenseCategorySummary>> GetCategoryBreakdownAsync(int year, int month, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalExpensesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }

    public class MonthlyExpenseSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public IReadOnlyList<ExpenseCategorySummary> Categories { get; set; } = Array.Empty<ExpenseCategorySummary>();
    }

    public class ExpenseCategorySummary
    {
        public int ExpenseTypeId { get; set; }
        public string ExpenseTypeCode { get; set; } = string.Empty;
        public string ExpenseTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}
