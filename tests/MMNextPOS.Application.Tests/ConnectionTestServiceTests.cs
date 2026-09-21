using System;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MySqlConnector;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    /// <summary>
    /// Unit tests for ConnectionTestService.
    /// </summary>
    public class ConnectionTestServiceTests
    {
        [Fact]
        public async Task TestConnectionAsync_InvalidString_ReturnsFailure()
        {
            // Arrange
            var service = CreateService("Invalid connection string");

            // Act
            var result = await service.TestConnectionAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task TestConnectionAsync_EmptyString_ReturnsFailure()
        {
            // Arrange
            var service = CreateService(string.Empty);

            // Act
            var result = await service.TestConnectionAsync();

            // Assert
            Assert.False(result.Success);
            Assert.True(result.LatencyMs > 0); // Time is always measured
        }

        [Fact]
        public async Task GetServerInfoAsync_InvalidString_ThrowsMySqlException()
        {
            // Arrange
            var service = CreateService("Server=localhost;Port=9999;Database=nonexistent;Uid=fake;Pwd=fake;ConnectionTimeout=1");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MySqlException>(() => service.GetServerInfoAsync());
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task GetTableCountAsync_InvalidString_ThrowsMySqlException()
        {
            // Arrange
            var service = CreateService("Server=localhost;Port=9999;Database=nonexistent;Uid=fake;Pwd=fake;ConnectionTimeout=1");

            // Act & Assert
            await Assert.ThrowsAsync<MySqlException>(() => service.GetTableCountAsync());
        }

        [Fact]
        public async Task GetDatabaseSizeAsync_InvalidString_ThrowsMySqlException()
        {
            // Arrange
            var service = CreateService("Server=localhost;Port=9999;Database=nonexistent;Uid=fake;Pwd=fake;ConnectionTimeout=1");

            // Act & Assert
            await Assert.ThrowsAsync<MySqlException>(() => service.GetDatabaseSizeBytesAsync());
        }

        private static IConnectionTestService CreateService(string connectionString)
        {
            return new ConnectionTestService(connectionString, new MockAuditService());
        }

        private class MockAuditService : IAuditService
        {
            public Task LogAsync(string entityName, int entityId, string action, object? oldValues, object? newValues,
                int? userId, string? userName, string? description, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }
    }
}
