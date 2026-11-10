using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class ExpenseSummaryServiceTests
    {
        private readonly Mock<IExpenseRepository> _expenseRepoMock = new();
        private readonly Mock<IExpenseTypeRepository> _expenseTypeRepoMock = new();

        private IExpenseSummaryService CreateService() => new ExpenseSummaryService(
            _expenseRepoMock.Object, _expenseTypeRepoMock.Object);

        private static Expense NewExpense(int id, int typeId, DateTime date, decimal amount, string status = "Active") => new()
        {
            Id = id,
            ExpenseTypeId = typeId,
            ExpenseDate = date,
            Amount = amount,
            Status = status
        };

        private static ExpenseType NewExpenseType(int id, string code, string name) => new() { Id = id, Code = code, Name = name };

        [Fact]
        public async Task GetMonthlySummaryAsync_HappyPath_ReturnsGroupedCategories()
        {
            // Arrange
            var year = 2026;
            var month = 3;
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            var expenses = new List<Expense>
            {
                NewExpense(1, 1, new DateTime(2026, 3, 5), 100m),
                NewExpense(2, 1, new DateTime(2026, 3, 10), 50m),   // same type → 150 total
                NewExpense(3, 2, new DateTime(2026, 3, 15), 200m),
                NewExpense(4, 3, new DateTime(2026, 2, 20), 999m),  // different month → excluded
                NewExpense(5, 1, new DateTime(2026, 3, 25), 25m, "Cancelled") // non-Active → excluded
            };
            var expenseTypes = new List<ExpenseType>
            {
                NewExpenseType(1, "RENT", "Rent"),
                NewExpenseType(2, "UTIL", "Utilities"),
                NewExpenseType(3, "MISC", "Misc")
            };

            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expenses);
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expenseTypes);

            var service = CreateService();

            // Act
            var summary = await service.GetMonthlySummaryAsync(year, month);

            // Assert
            Assert.Equal(year, summary.Year);
            Assert.Equal(month, summary.Month);
            Assert.Equal(350m, summary.TotalAmount);   // 100 + 50 + 200 (25 cancelled excluded)
            Assert.Equal(3, summary.TransactionCount);  // 3 active March expenses
            Assert.Equal(2, summary.Categories.Count);  // RENT + UTILITIES only
            var rent = summary.Categories.First(c => c.ExpenseTypeId == 1);
            Assert.Equal(150m, rent.Amount);
            Assert.Equal("RENT", rent.ExpenseTypeCode);
            Assert.Equal("Rent", rent.ExpenseTypeName);
            Assert.Equal(2, rent.Count);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_NoExpenses_ReturnsZeroSummary()
        {
            // Arrange
            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Expense>());
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ExpenseType>());

            var service = CreateService();

            // Act
            var summary = await service.GetMonthlySummaryAsync(2026, 1);

            // Assert
            Assert.Equal(0m, summary.TotalAmount);
            Assert.Equal(0, summary.TransactionCount);
            Assert.Empty(summary.Categories);
        }

        [Fact]
        public async Task GetMonthlySummaryAsync_UnknownExpenseType_ReturnsUnk()
        {
            // Arrange
            var expenses = new List<Expense> { NewExpense(1, 99, new DateTime(2026, 4, 1), 50m) };
            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expenses);
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ExpenseType>());

            var service = CreateService();

            // Act
            var summary = await service.GetMonthlySummaryAsync(2026, 4);

            // Assert
            var cat = Assert.Single(summary.Categories);
            Assert.Equal("UNK", cat.ExpenseTypeCode);
            Assert.Equal("Unknown", cat.ExpenseTypeName);
            Assert.Equal(50m, cat.Amount);
        }

        [Fact]
        public async Task GetYearlySummaryAsync_ReturnsTwelveMonths()
        {
            // Arrange
            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Expense>());
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ExpenseType>());

            var service = CreateService();

            // Act
            var summaries = await service.GetYearlySummaryAsync(2026);

            // Assert
            Assert.Equal(12, summaries.Count());
            Assert.All(summaries, s => Assert.Equal(2026, s.Year));
            Assert.Equal(1, summaries.First().Month);
            Assert.Equal(12, summaries.Last().Month);
        }

        [Fact]
        public async Task GetCategoryBreakdownAsync_ReturnsMonthlyCategories()
        {
            // Arrange
            var expenses = new List<Expense> { NewExpense(1, 1, new DateTime(2026, 5, 1), 75m) };
            var types = new List<ExpenseType> { NewExpenseType(1, "MISC", "Misc") };
            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expenses);
            _expenseTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(types);

            var service = CreateService();

            // Act
            var breakdown = await service.GetCategoryBreakdownAsync(2026, 5);

            // Assert
            Assert.Single(breakdown);
            Assert.Equal(75m, breakdown.First().Amount);
        }

        [Fact]
        public async Task GetTotalExpensesAsync_FiltersByDateRangeAndActiveStatus()
        {
            // Arrange
            var expenses = new List<Expense>
            {
                NewExpense(1, 1, new DateTime(2026, 1, 10), 100m),   // in range, Active
                NewExpense(2, 1, new DateTime(2026, 1, 20), 50m),    // in range, Active
                NewExpense(3, 1, new DateTime(2025, 12, 31), 999m),  // out of range
                NewExpense(4, 1, new DateTime(2026, 1, 15), 25m, "Cancelled") // in range but cancelled
            };
            _expenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expenses);

            var service = CreateService();

            // Act
            var total = await service.GetTotalExpensesAsync(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));

            // Assert — only the two Active January expenses: 100 + 50 = 150
            Assert.Equal(150m, total);
        }
    }
}
