using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    /// <summary>
    /// Unit tests for ReportService: report menu management and report generation.
    /// </summary>
    public class ReportServiceTests
    {
        private readonly Mock<IReportMenusRepository> _reportMenuRepoMock = new();
        private readonly Mock<IStarCashFlowReportRepository> _cashFlowRepoMock = new();
        private readonly Mock<IStarProfitLossReportRepository> _profitLossRepoMock = new();
        private readonly Mock<IStarStockBalanceReportRepository> _stockBalanceRepoMock = new();
        private readonly Mock<IStarReorderReportRepository> _reorderRepoMock = new();
        private readonly Mock<IStarOutstandingReportRepository> _outstandingRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IReportService CreateService()
        {
            return new ReportService(
                _reportMenuRepoMock.Object,
                _cashFlowRepoMock.Object,
                _profitLossRepoMock.Object,
                _stockBalanceRepoMock.Object,
                _reorderRepoMock.Object,
                _outstandingRepoMock.Object,
                _auditServiceMock.Object);
        }

        private void SetupHappyPath()
        {
            _reportMenuRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ReportMenus>
                {
                    new() { Id = 1, Code = "RPT-001", Name = "Sales Report", IsReport = true },
                    new() { Id = 2, Code = "RPT-002", Name = "Purchase Report", IsReport = true }
                });
            _reportMenuRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => new ReportMenus { Id = id, Code = "RPT-001", Name = "Test Report" });
            _reportMenuRepoMock.Setup(r => r.AddAsync(It.IsAny<ReportMenus>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReportMenus rm, CancellationToken _) => { rm.Id = 1; return rm; });
            _reportMenuRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ReportMenus>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _reportMenuRepoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // Report Menus
        [Fact]
        public async Task GetReportMenuByIdAsync_ExistingMenu_ReturnsMenu()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetReportMenuByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("RPT-001", result.Code);
        }

        [Fact]
        public async Task GetReportMenusAsync_AllMenus_ReturnsAllMenus()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetReportMenusAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetReportMenusAsync_ReportsOnly_FiltersCorrectly()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetReportMenusAsync(includeReportsOnly: true);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddReportMenuAsync_ValidMenu_CreatesMenu()
        {
            SetupHappyPath();
            var service = CreateService();
            var menu = new ReportMenus { Code = "RPT-999", Name = "New Report", IsReport = true };

            var result = await service.AddReportMenuAsync(menu);

            Assert.Equal(1, result.Id);
            _reportMenuRepoMock.Verify(r => r.AddAsync(It.IsAny<ReportMenus>(), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(ReportMenus), 1, "Create", null, It.IsAny<ReportMenus>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReportMenuAsync_ExistingMenu_UpdatesMenu()
        {
            SetupHappyPath();
            var service = CreateService();
            var menu = new ReportMenus { Id = 1, Code = "RPT-001", Name = "Updated Name", IsReport = false };

            await service.UpdateReportMenuAsync(menu);

            _reportMenuRepoMock.Verify(r => r.UpdateAsync(It.Is<ReportMenus>(m => m.Name == "Updated Name"), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(ReportMenus), 1, "Update", It.IsAny<object>(), It.IsAny<ReportMenus>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteReportMenuAsync_ExistingMenu_DeletesMenu()
        {
            SetupHappyPath();
            var service = CreateService();

            await service.DeleteReportMenuAsync(1);

            _reportMenuRepoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(ReportMenus), 1, "Delete", It.IsAny<object>(), null, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // Starman Reports - Cash Flow
        [Fact]
        public async Task GetCashFlowReportsAsync_ByLocationAndDateRange_ReturnsFiltered()
        {
            var report1 = new StarCashFlowReport { Id = 1, LocationId = 1, ReportDate = DateTime.UtcNow.AddDays(-5) };
            var report2 = new StarCashFlowReport { Id = 2, LocationId = 1, ReportDate = DateTime.UtcNow.AddDays(-2) };
            var report3 = new StarCashFlowReport { Id = 3, LocationId = 2, ReportDate = DateTime.UtcNow.AddDays(-3) };

            _cashFlowRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StarCashFlowReport> { report1, report2, report3 });
            var service = CreateService();

            var result = await service.GetCashFlowReportsAsync(1, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

            Assert.Equal(2, result.Count);
        }

        // Starman Reports - Profit/Loss
        [Fact]
        public async Task GetProfitLossReportsAsync_ByLocationAndDateRange_ReturnsFiltered()
        {
            var report1 = new StarProfitLossReport { Id = 1, LocationId = 1, FromDate = DateTime.UtcNow.AddDays(-5), ToDate = DateTime.UtcNow.AddDays(-1) };
            var report2 = new StarProfitLossReport { Id = 2, LocationId = 2, FromDate = DateTime.UtcNow.AddDays(-5), ToDate = DateTime.UtcNow.AddDays(-1) };

            _profitLossRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StarProfitLossReport> { report1, report2 });
            var service = CreateService();

            var result = await service.GetProfitLossReportsAsync(1, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

            Assert.Single(result);
        }

        // Stock Balance
        [Fact]
        public async Task GetStockBalanceReportsAsync_ByLocation_ReturnsFiltered()
        {
            var report1 = new StarStockBalanceReport { Id = 1, LocationId = 1, ProductName = "Widget" };
            var report2 = new StarStockBalanceReport { Id = 2, LocationId = 1, ProductName = "Gadget" };

            _stockBalanceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StarStockBalanceReport> { report1, report2 });
            var service = CreateService();

            var result = await service.GetStockBalanceReportsAsync(1, DateTime.UtcNow);

            Assert.Equal(2, result.Count);
        }

        // Reorder Reports
        [Fact]
        public async Task GetReorderReportsAsync_ByLocation_ReturnsFiltered()
        {
            var report1 = new StarReorderReport { Id = 1, LocationId = 1, ProductName = "Widget" };
            var report2 = new StarReorderReport { Id = 2, LocationId = 2, ProductName = "Gadget" };

            _reorderRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StarReorderReport> { report1, report2 });
            var service = CreateService();

            var result = await service.GetReorderReportsAsync(1, DateTime.UtcNow);

            Assert.Single(result);
            Assert.Equal("Widget", result[0].ProductName);
        }

        // Outstanding Reports
        [Fact]
        public async Task GetOutstandingReportsAsync_ByLocation_ReturnsFiltered()
        {
            var report1 = new StarOutstandingReport { Id = 1, LocationId = 1, PartyName = "Customer A" };
            var report2 = new StarOutstandingReport { Id = 2, LocationId = 2, PartyName = "Customer B" };

            _outstandingRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StarOutstandingReport> { report1, report2 });
            var service = CreateService();

            var result = await service.GetOutstandingReportsAsync(2, DateTime.UtcNow);

            Assert.Single(result);
            Assert.Equal("Customer B", result[0].PartyName);
        }

        // Generic Report Generation
        [Fact]
        public async Task GenerateReportAsync_BasicGeneration_ReturnsByteArray()
        {
            SetupHappyPath();
            var service = CreateService();
            var parameters = new Dictionary<string, object> { { "TestParam", "TestValue" } };

            var result = await service.GenerateReportAsync("TestReport", parameters);

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        // Sale Receipt - NotImplemented (WinForms only)
        [Fact]
        public async Task GenerateSaleReceiptAsync_ThrowsNotImplemented_InBaseLayer()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<NotImplementedException>(() => service.GenerateSaleReceiptAsync(1));
        }

        // Daily Sale Summary - NotImplemented (WinForms only)
        [Fact]
        public async Task GenerateDailySaleSummaryAsync_ThrowsNotImplemented_InBaseLayer()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<NotImplementedException>(() => service.GenerateDailySaleSummaryAsync(DateTime.UtcNow));
        }
    }
}
