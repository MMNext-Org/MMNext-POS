using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MMNextPOS.Application.Services;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.WinForms.Services;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// Integration tests for WinFormsReportService to verify actual report generation works correctly.
    /// These tests validate that the DevExpress XtraReports implementation functions as expected.
    /// </summary>
    [Collection(nameof(MySqlContainerCollection))]
    public class WinFormsReportServiceIntegrationTests
    {
        private readonly MySqlContainerFixture _fixture;

        public WinFormsReportServiceIntegrationTests(MySqlContainerFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<IReportService> CreateWinFormsReportService()
        {
            // Set up a service provider that includes WinFormsReportService
            var services = new ServiceCollection();

            // Add infrastructure services
            services.AddScoped<IReportMenusRepository, ReportMenusRepository>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IServiceProvider>(sp => sp);

            // Add WinFormsReportService (this is what we're testing)
            services.AddTransient<WinFormsReportService>();

            // Add application services needed by WinFormsReportService
            services.AddTransient<ISalesService, SalesService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<ITranslationService, TranslationService>();
            services.AddTransient<IServiceProvider>(sp => sp);

            return await services.BuildServiceProviderAsync();
        }

        [Fact]
        public async Task GenerateReportAsync_ValidReportName_ReturnsPdfBytes()
        {
            // Arrange
            var serviceProvider = await CreateWinFormsReportService();
            var service = serviceProvider.GetRequiredService<WinFormsReportService>();

            // Act
            var parameters = new Dictionary<string, object> { { "TestParam", "TestValue" } };
            var result = await service.GenerateReportAsync("DailySaleSummary", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateSaleReceiptAsync_ValidSaleId_ReturnsPdfBytes()
        {
            // Arrange
            var serviceProvider = await CreateWinFormsReportService();
            var service = serviceProvider.GetRequiredService<WinFormsReportService>();

            // Act
            var result = await service.GenerateSaleReceiptAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateReportAsync_NonExistentReport_ReturnsBytes()
        {
            // Arrange
            var serviceProvider = await CreateWinFormsReportService();
            var service = serviceProvider.GetRequiredService<WinFormsReportService>();

            // Act
            var parameters = new Dictionary<string, object> { { "TestParam", "TestValue" } };
            var result = await service.GenerateReportAsync("NonExistentReport", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateReportAsync_NullParameters_ReturnsPdfBytes()
        {
            // Arrange
            var serviceProvider = await CreateWinFormsReportService();
            var service = serviceProvider.GetRequiredService<WinFormsReportService>();

            // Act
            var result = await service.GenerateReportAsync("DailySaleSummary", null!);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateSaleReceiptAsync_ZeroSaleId_ReturnsPdfBytes()
        {
            // Arrange
            var serviceProvider = await CreateWinFormsReportService();
            var service = serviceProvider.GetRequiredService<WinFormsReportService>();

            // Act
            var result = await service.GenerateSaleReceiptAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateDailySaleSummaryAsync_ValidDate_ReturnsPdfBytes()
        {
            // Arrange
            var serviceProvider = await CreateWinFormsReportService();
            var service = serviceProvider.GetRequiredService<WinFormsReportService>();

            // Act
            var result = await service.GenerateDailySaleSummaryAsync(DateTime.UtcNow);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}
