using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class ExpenseSummaryService : IExpenseSummaryService
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IExpenseTypeRepository _expenseTypeRepo;

        public ExpenseSummaryService(
            IExpenseRepository expenseRepo,
            IExpenseTypeRepository expenseTypeRepo)
        {
            _expenseRepo = expenseRepo ?? throw new ArgumentNullException(nameof(expenseRepo));
            _expenseTypeRepo = expenseTypeRepo ?? throw new ArgumentNullException(nameof(expenseTypeRepo));
        }

        public async Task<MonthlyExpenseSummary> GetMonthlySummaryAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            var allExpenses = await _expenseRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var monthlyExpenses = allExpenses
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate && e.Status == "Active")
                .ToList();

            var expenseTypes = (await _expenseTypeRepo.GetAllAsync(cancellationToken).ConfigureAwait(false))
                .ToDictionary(t => t.Id, t => t);

            var categoryGroups = monthlyExpenses
                .GroupBy(e => e.ExpenseTypeId)
                .Select(g => new ExpenseCategorySummary
                {
                    ExpenseTypeId = g.Key,
                    ExpenseTypeCode = expenseTypes.TryGetValue(g.Key, out var et) ? et.Code : "UNK",
                    ExpenseTypeName = expenseTypes.TryGetValue(g.Key, out et) ? et.Name : "Unknown",
                    Amount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            var totalAmount = monthlyExpenses.Sum(e => e.Amount);

            foreach (var cat in categoryGroups)
            {
                cat.Percentage = totalAmount > 0 ? Math.Round(cat.Amount / totalAmount * 100, 2) : 0;
            }

            return new MonthlyExpenseSummary
            {
                Year = year,
                Month = month,
                TotalAmount = totalAmount,
                TransactionCount = monthlyExpenses.Count,
                Categories = categoryGroups
            };
        }

        public async Task<IReadOnlyList<MonthlyExpenseSummary>> GetYearlySummaryAsync(int year, CancellationToken cancellationToken = default)
        {
            var summaries = new List<MonthlyExpenseSummary>();

            for (int month = 1; month <= 12; month++)
            {
                var summary = await GetMonthlySummaryAsync(year, month, cancellationToken).ConfigureAwait(false);
                summaries.Add(summary);
            }

            return summaries;
        }

        public async Task<IReadOnlyList<ExpenseCategorySummary>> GetCategoryBreakdownAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            var summary = await GetMonthlySummaryAsync(year, month, cancellationToken).ConfigureAwait(false);
            return summary.Categories;
        }

        public async Task<decimal> GetTotalExpensesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var allExpenses = await _expenseRepo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return allExpenses
                .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate && e.Status == "Active")
                .Sum(e => e.Amount);
        }
    }
}
