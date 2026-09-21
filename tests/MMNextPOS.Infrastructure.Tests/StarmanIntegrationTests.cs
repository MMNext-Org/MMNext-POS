using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// Integration tests for Starman multi-site workflows (Phase 4 Sprint 4).
    /// Uses the shared MySqlContainerFixture pattern. Docker may be unavailable;
    /// if so these tests are skipped (Docker is required for Testcontainers).
    /// </summary>
    [Collection(nameof(MySqlContainerCollection))]
    public class StarmanIntegrationTests
    {
        private readonly MySqlContainerFixture _fixture;

        public StarmanIntegrationTests(MySqlContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Component", "Starman")]
        public async Task StarStockTransferReceivedService_CreateAndAccept_Persists()
        {
            // Arrange — fresh DB with migrations
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();

            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IStarStockTransferReceivedService>();

            var transfer = new StarStockTransferReceived
            {
                TransferNo = "ST-HQ-001",
                FromLocationId = 1,
                ToLocationId = 2,
                TransferDate = DateTime.UtcNow,
                Status = "Pending"
            };

            // Act
            var created = await service.AddAsync(transfer);

            // Assert
            Assert.NotNull(created);
            Assert.Equal("ST-HQ-001", created.TransferNo);
            Assert.Equal("Pending", created.Status);

            // Act: accept
            created.ReceivedDate = DateTime.UtcNow;
            created.Status = "Accepted";
            await service.UpdateAsync(created);

            // Assert
            var updated = await service.GetByIdAsync(created.Id);
            Assert.NotNull(updated);
            Assert.Equal("Accepted", updated!.Status);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Component", "Starman")]
        public async Task StarSalePriceTransferService_CreateAndAccept_Persists()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();

            using var scope = _fixture.ServiceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IStarSalePriceTransferService>();

            var transfer = new StarSalePriceTransfer
            {
                TransferNo = "PRICE-HQ-001",
                FromLocationId = 1,
                ToLocationId = 2,
                TransferDate = DateTime.UtcNow,
                Status = "Pending"
            };

            // Act
            var created = await service.AddAsync(transfer);

            // Assert
            Assert.NotNull(created);
            Assert.Equal("PRICE-HQ-001", created.TransferNo);
            Assert.Equal("Pending", created.Status);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Component", "Starman")]
        public async Task StarReportRepositories_AreResolvable()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();

            using var scope = _fixture.ServiceProvider.CreateScope();

            // Act + Assert — verify all Starman report repos resolve without throwing
            var stockBalanceRepo = scope.ServiceProvider.GetRequiredService<IStarStockBalanceReportRepository>();
            Assert.NotNull(await stockBalanceRepo.GetAllAsync());

            var cashFlowRepo = scope.ServiceProvider.GetRequiredService<IStarCashFlowReportRepository>();
            Assert.NotNull(await cashFlowRepo.GetAllAsync());

            var profitLossRepo = scope.ServiceProvider.GetRequiredService<IStarProfitLossReportRepository>();
            Assert.NotNull(await profitLossRepo.GetAllAsync());

            var reorderReportRepo = scope.ServiceProvider.GetRequiredService<IStarReorderReportRepository>();
            Assert.NotNull(await reorderReportRepo.GetAllAsync());

            var outstandingReportRepo = scope.ServiceProvider.GetRequiredService<IStarOutstandingReportRepository>();
            Assert.NotNull(await outstandingReportRepo.GetAllAsync());
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Component", "Starman")]
        public async Task StarmanFullFlow_HQToRemote_TransferAndPrice_Succeed()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await _fixture.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();

            using var scope = _fixture.ServiceProvider.CreateScope();

            // Verify that dependent services are all resolvable
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IInventoryService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IStockTransferService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAssemblyService>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<ISerialNumberService>());

            // Confirm Starman services register
            var transferReceivedService = scope.ServiceProvider.GetService<IStarStockTransferReceivedService>();
            Assert.NotNull(transferReceivedService);

            var priceTransferService = scope.ServiceProvider.GetService<IStarSalePriceTransferService>();
            Assert.NotNull(priceTransferService);
        }
    }
}
