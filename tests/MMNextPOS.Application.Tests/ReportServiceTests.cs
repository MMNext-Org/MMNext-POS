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
    public class LicenseInfoServiceTests
    {
        private readonly Mock<ILicenseInfoRepository> _licenseRepoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private ILicenseInfoService CreateService()
        {
            return new LicenseInfoService(_licenseRepoMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingLicense_ReturnsLicense()
        {
            var license = new LicenseInfo { Id = 1, LicenseKey = "LIC-001", CompanyName = "Test Co", Status = "Active" };
            _licenseRepoMock.Setup(r => r.GetByIdAsync(license.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(license);

            var service = CreateService();
            var result = await service.GetByIdAsync(license.Id);

            Assert.NotNull(result);
            Assert.Equal(license.Id, result.Id);
            Assert.Equal(license.LicenseKey, result.LicenseKey);
            Assert.Equal(license.CompanyName, result.CompanyName);
        }

        [Fact]
        public async Task GetByLicenseKeyAsync_ExistingLicense_ReturnsLicense()
        {
            var license = new LicenseInfo { Id = 1, LicenseKey = "LIC-001", CompanyName = "Test Co" };
            var licenses = new List<LicenseInfo> { license };
            _licenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(licenses);

            var service = CreateService();
            var result = await service.GetByLicenseKeyAsync("LIC-001");

            Assert.NotNull(result);
            Assert.Equal(license.Id, result.Id);
            Assert.Equal(license.LicenseKey, result.LicenseKey);
        }

        [Fact]
        public async Task GetByLicenseKeyAsync_NonExistentLicense_ReturnsNull()
        {
            _licenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<LicenseInfo>());

            var service = CreateService();
            var result = await service.GetByLicenseKeyAsync("NON-EXISTENT");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllLicenses()
        {
            var licenses = new List<LicenseInfo>
            {
                new() { Id = 1, LicenseKey = "LIC-001", CompanyName = "Company A" },
                new() { Id = 2, LicenseKey = "LIC-002", CompanyName = "Company B" },
            };
            _licenseRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(licenses);

            var service = CreateService();
            var result = await service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task AddAsync_ValidLicense_ReturnsAddedLicense()
        {
            var license = new LicenseInfo { LicenseKey = "LIC-003", CompanyName = "New Co", ExpiryDate = DateTime.Today.AddYears(1) };
            var addedLicense = new LicenseInfo { Id = 3, LicenseKey = "LIC-003", CompanyName = "New Co" };
            _licenseRepoMock.Setup(r => r.AddAsync(license, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(addedLicense);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var result = await service.AddAsync(license);

            Assert.NotNull(result);
            Assert.Equal(addedLicense.Id, result.Id);
            Assert.Equal(license.LicenseKey, result.LicenseKey);
            _licenseRepoMock.Verify(r => r.AddAsync(license, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingLicense_UpdatesLicense()
        {
            var existing = new LicenseInfo { Id = 1, LicenseKey = "LIC-001", CompanyName = "Old Co", Status = "Active" };
            var updated = new LicenseInfo { Id = 1, LicenseKey = "LIC-001", CompanyName = "New Co", Status = "Expired" };
            _licenseRepoMock.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existing);
            _licenseRepoMock.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.UpdateAsync(updated);

            _licenseRepoMock.Verify(r => r.UpdateAsync(It.Is<LicenseInfo>(l => l.CompanyName == "New Co" && l.Status == "Expired"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingLicense_DeletesLicense()
        {
            var license = new LicenseInfo { Id = 1, LicenseKey = "LIC-001", CompanyName = "Test Co" };
            _licenseRepoMock.Setup(r => r.GetByIdAsync(license.Id, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(license);
            _licenseRepoMock.Setup(r => r.DeleteAsync(license.Id, It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            await service.DeleteAsync(license.Id);

            _licenseRepoMock.Verify(r => r.DeleteAsync(license.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsException_WhenRepositoryThrows()
        {
            var ex = new InvalidOperationException("DB error");
            _licenseRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ThrowsAsync(ex);

            var service = CreateService();
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByIdAsync(1));
        }
    }

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
                Mock.Of<IReportMenusRepository>(),
                Mock.Of<IStarCashFlowReportRepository>(),
                Mock.Of<IStarProfitLossReportRepository>(),
                Mock.Of<IStarStockBalanceReportRepository>(),
                Mock.Of<IStarReorderReportRepository>(),
                Mock.Of<IStarOutstandingReportRepository>(),
                Mock.Of<IAuditService>());
        }

        [Fact]
        public async Task GetReportMenusAsync_ReturnsAllMenus()
        {
            var menus = new List<ReportMenus>
            {
                new() { Id = 1, Code = "RPT001", Name = "Sales Report", IsReport = true },
                new() { Id = 2, Code = "MNU001", Name = "Main Menu", IsReport = false },
            };
            var repoMock = new Mock<IReportMenusRepository>();
            repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(menus);

            var service = new ReportService(
                repoMock.Object,
                Mock.Of<IStarCashFlowReportRepository>(),
                Mock.Of<IStarProfitLossReportRepository>(),
                Mock.Of<IStarStockBalanceReportRepository>(),
                Mock.Of<IStarReorderReportRepository>(),
                Mock.Of<IStarOutstandingReportRepository>(),
                Mock.Of<IAuditService>());

            var result = await service.GetReportMenusAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetReportMenusAsync_FiltersByReportsOnly()
        {
            var menus = new List<ReportMenus>
            {
                new() { Id = 1, Code = "RPT001", Name = "Sales Report", IsReport = true },
                new() { Id = 2, Code = "MNU001", Name = "Main Menu", IsReport = false },
            };
            var repoMock = new Mock<IReportMenusRepository>();
            repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(menus);

            var service = new ReportService(
                repoMock.Object,
                Mock.Of<IStarCashFlowReportRepository>(),
                Mock.Of<IStarProfitLossReportRepository>(),
                Mock.Of<IStarStockBalanceReportRepository>(),
                Mock.Of<IStarReorderReportRepository>(),
                Mock.Of<IStarOutstandingReportRepository>(),
                Mock.Of<IAuditService>());

            var result = await service.GetReportMenusAsync(includeReportsOnly: true);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result[0].IsReport);
        }

        [Fact]
        public async Task AddReportMenuAsync_ValidMenu_ReturnsAddedMenu()
        {
            var menu = new ReportMenus { Code = "RPT003", Name = "New Report", IsReport = true };
            var addedMenu = new ReportMenus { Id = 3, Code = "RPT003", Name = "New Report", IsReport = true };
            var repoMock = new Mock<IReportMenusRepository>();
            repoMock.Setup(r => r.AddAsync(It.IsAny<ReportMenus>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(addedMenu);
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new ReportService(
                repoMock.Object,
                Mock.Of<IStarCashFlowReportRepository>(),
                Mock.Of<IStarProfitLossReportRepository>(),
                Mock.Of<IStarStockBalanceReportRepository>(),
                Mock.Of<IStarReorderReportRepository>(),
                Mock.Of<IStarOutstandingReportRepository>(),
                Mock.Of<IAuditService>());

            var result = await service.AddReportMenuAsync(new ReportMenus { Code = "RPT003", Name = "New Report" });

            Assert.NotNull(result);
            Assert.Equal(addedMenu.Id, result.Id);
            Assert.Equal(addedMenu.Code, result.Code);
        }

        [Fact]
        public async Task GetCashFlowReportsAsync_FiltersByLocationAndDate()
        {
            var reports = new List<StarCashFlowReport>
            {
                new() { Id = 1, LocationId = 1, ReportDate = new DateTime(2024, 1, 15), TotalSales = 1000m },
                new() { Id = 2, LocationId = 1, ReportDate = new DateTime(2024, 1, 20), TotalSales = 2000m },
                new() { Id = 3, LocationId = 2, ReportDate = new DateTime(2024, 1, 15), TotalSales = 1500m },
            };
            _cashFlowRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                             .ReturnsAsync(reports);

            var service = new ReportService(
                Mock.Of<IReportMenusRepository>(),
                _cashFlowRepoMock.Object,
                Mock.Of<IStarProfitLossReportRepository>(),
                Mock.Of<IStarStockBalanceReportRepository>(),
                Mock.Of<IStarReorderReportRepository>(),
                Mock.Of<IStarOutstandingReportRepository>(),
                Mock.Of<IAuditService>());

            var result = await service.GetCashFlowReportsAsync(1, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(1, r.LocationId));
        }

        [Fact]
        public async Task GetProfitLossReportsAsync_FiltersByLocationAndDateRange()
        {
            var reports = new List<StarProfitLossReport>
            {
                new() { Id = 1, LocationId = 1, FromDate = new DateTime(2024, 1, 1), ToDate = new DateTime(2024, 1, 31), NetProfit = 1000m },
                new() { Id = 2, LocationId = 1, FromDate = new DateTime(2024, 2, 1), ToDate = new DateTime(2024, 2, 29), NetProfit = 1500m },
            };
            _profitLossRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                               .ReturnsAsync(reports);

            var service = new ReportService(
                Mock.Of<IReportMenusRepository>(),
                Mock.Of<IStarCashFlowReportRepository>(),
                _profitLossRepoMock.Object,
                Mock.Of<IStarStockBalanceReportRepository>(),
                Mock.Of<IStarReorderReportRepository>(),
                Mock.Of<IStarOutstandingReportRepository>(),
                Mock.Of<IAuditService>());

            var result = await service.GetProfitLossReportsAsync(1, new DateTime(2024, 1, 1), new DateTime(2024, 2, 29));

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetStockBalanceReportsAsync_FiltersByLocationAndDate()
        {
            var reports = new List<StarStockBalanceReport>
            {
                new() { Id = 1, LocationId = 1, ProductId = 1, ProductName = "Product A", QuantityOnHand = 100, LastMovementDate = new DateTime(2024, 1, 15) },
                new() { Id = 2, LocationId = 1, ProductId = 2, ProductName = "Product B", QuantityOnHand = 50, LastMovementDate = new DateTime(2024, 1, 20) },
            };
            _stockBalanceRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(reports);

            var service = new ReportService(
                Mock.Of<IReportMenusRepository>(),
                Mock.Of<IStarCashFlowReportRepository>(),
                Mock.Of<IStarProfitLossReportRepository>(),
                _stockBalanceRepoMock.Object,
                Mock.Of<IStarReorderReportRepository>(),
                Mock.Of<IStarOutstandingReportRepository>(),
                Mock.Of<IAuditService>());

            var result = await service.GetStockBalanceReportsAsync(1, new DateTime(2024, 1, 31));

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GenerateReportAsync_ReturnsPlaceholderBytes()
        {
            var service = new ReportService(
                Mock.Of<IReportMenusRepository>(),
                Mock.Of<IStarCashFlowReportRepository>(),
                Mock.Of<IStarProfitLossReportRepository>(),
                Mock.Of<IStarStockBalanceReportRepository>(),
                Mock.Of<IStarReorderReportRepository>(),
                Mock.Of<IStarOutstandingReportRepository>(),
                Mock.Of<IAuditService>());

            var result = await service.GenerateReportAsync("TestReport", new Dictionary<string, object> { { "param1", "value1" } });

            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            var resultStr = System.Text.Encoding.UTF8.GetString(result);
            Assert.Contains("TestReport", resultStr);
        }
    }
}
