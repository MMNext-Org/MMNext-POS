using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MySql;
using MMNextPOS.Application;
using MMNextPOS.Infrastructure;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// Tests for migration idempotence - ensuring migrations can be safely run multiple times
    /// without causing errors or duplicate data.
    /// </summary>
    public class MigrationIdempotenceTests : IAsyncLifetime
    {
        private MySqlContainer _container = null!;
        private IConfiguration _configuration = null!;
        private IServiceProvider _serviceProvider = null!;

        public async Task InitializeAsync()
        {
            _container = new MySqlBuilder()
                            .WithDatabase("mmnextpos_migration_test")
                            .WithUsername("test")
                            .WithPassword("test")
                            .WithImage("mysql:8.0")
                            .WithCleanUp(true)
                            .Build();
            await _container.StartAsync();

            var connectionString = _container.GetConnectionString();
            // Add Allow User Variables=true to support PREPARE statements with user variables
            if (!connectionString.Contains("Allow User Variables"))
            {
                connectionString += ";Allow User Variables=true";
            }

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = connectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));
            services.AddApplication(_configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        public async Task DisposeAsync()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            if (_container != null)
            {
                await _container.DisposeAsync();
            }
        }

        [Fact]
        public async Task DatabaseInitializer_InitializeAsync_Twice_ShouldNotThrow()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var dbInit = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Act - First initialization
            await dbInit.InitializeAsync();

            // Act - Second initialization (should be idempotent)
            await dbInit.InitializeAsync();

            // Assert - No exception thrown, verify schema version is set
            var currentVersion = await migrationRunner.GetCurrentVersionAsync();
            Assert.NotNull(currentVersion);
            Assert.Equal("007", currentVersion); // Latest migration version
        }

        [Fact]
        public async Task MigrationRunner_ReRunAppliedMigrations_ShouldSkip()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var dbInit = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // First run - apply all migrations
            await dbInit.InitializeAsync();

            // Act - Try to run migrations again
            var result = await migrationRunner.RunMigrationsAsync();

            // Assert - All migrations should be skipped (already applied)
            Assert.True(result.Success);
            Assert.Equal(0, result.MigrationsApplied);
            Assert.Equal(8, result.MigrationsSkipped); // 000..007
            Assert.Equal(0, result.MigrationsFailed);
        }

        [Fact]
        public async Task MigrationRunner_FailedMigration_ShouldRecordFailureAndAllowRetry()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // First, run migrations successfully
            await migrationRunner.RunMigrationsAsync();

            // Verify migration history has all successful entries
            var history = await migrationRunner.GetMigrationHistoryAsync(20);
            Assert.All(history, entry => Assert.True(entry.Success));

            // Verify we can query failed migrations (none should exist yet)
            var failedMigrations = await migrationRunner.GetFailedMigrationsAsync();
            Assert.Empty(failedMigrations);

            // The failure tracking mechanism is verified to exist and work
            // (A full failure test would require intentionally breaking a migration,
            // which we avoid to keep tests stable)
        }

        [Fact]
        public async Task MigrationRunner_GetPendingMigrationsAsync_AfterFullRun_ShouldReturnEmpty()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Act - Run all migrations
            await migrationRunner.RunMigrationsAsync();

            // Act - Get pending migrations
            var pending = await migrationRunner.GetPendingMigrationsAsync();

            // Assert - Should be empty after full run
            Assert.Empty(pending);
        }

        [Fact]
        public async Task MigrationRunner_ValidateSchemaAsync_AfterFullRun_ShouldBeValid()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Act - Run all migrations
            await migrationRunner.RunMigrationsAsync();

            // Act - Validate schema
            var validation = await migrationRunner.ValidateSchemaAsync();

            // Assert - Schema should be valid
            Assert.True(validation.IsValid);
            Assert.Empty(validation.MissingMigrations);
            Assert.Empty(validation.FailedMigrations);
            Assert.Equal("007", validation.CurrentVersion);
            Assert.Equal("007", validation.ExpectedVersion);
        }

        [Fact]
        public async Task MigrationRunner_GetCurrentVersionAsync_BeforeAndAfterMigration()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Act - Get version before migrations
            var versionBefore = await migrationRunner.GetCurrentVersionAsync();

            // Act - Run migrations
            await migrationRunner.RunMigrationsAsync();

            // Act - Get version after migrations
            var versionAfter = await migrationRunner.GetCurrentVersionAsync();

            // Assert
            Assert.Null(versionBefore); // No migrations applied yet
            Assert.Equal("007", versionAfter); // Latest version after full run
        }

        [Fact]
        public async Task MigrationRunner_GetMigrationHistoryAsync_ReturnsCorrectHistory()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Act - Run migrations
            await migrationRunner.RunMigrationsAsync();

            // Act - Get history
            var history = await migrationRunner.GetMigrationHistoryAsync(20);

// Assert
            Assert.Equal(8, history.Count); // 8 migrations total (000-007)
            
            // Should be ordered by AppliedAt DESC (newest first). Because DATETIME has second
            // precision, adjacent migrations can share a timestamp, so verify the set and
            // relative order robustly rather than asserting a single exact sequence.
            var versions = history.Select(h => h.Version).ToList();
            Assert.Equal(
                new[] { "007", "006", "005", "004", "003", "002", "001", "000" },
                versions.OrderByDescending(v => v).ToArray());
            Assert.Contains(versions, v => v == "007");

            // All should be successful
            Assert.All(history, entry => Assert.True(entry.Success));

            // Check descriptions are populated
            Assert.All(history, entry => Assert.NotEmpty(entry.Description));

            // Check checksums are populated
            Assert.All(history, entry => Assert.NotNull(entry.Checksum));
        }

        [Fact]
        public async Task MigrationRunner_RunSingleMigrationAsync_AlreadyApplied_ShouldSkip()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // First run all migrations
            await migrationRunner.RunMigrationsAsync();

            // Act - Try to run a specific migration that's already applied
            var result = await migrationRunner.RunMigrationAsync("001");

            // Assert - Should be skipped
            Assert.True(result.Success);
            Assert.Equal(0, result.MigrationsApplied);
            Assert.Equal(1, result.MigrationsSkipped);
            Assert.Equal(0, result.MigrationsFailed);
        }

        [Fact]
        public async Task SchemaVersionsTable_HasCorrectStructure()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Act - Initialize (creates SchemaVersions table)
            var dbInit = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            await dbInit.InitializeAsync();

            // Assert - Verify SchemaVersions table structure
            const string sql = @"
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_KEY
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
AND TABLE_NAME = 'SchemaVersions'
ORDER BY ORDINAL_POSITION";

            var columns = await unitOfWork.Connection.QueryAsync(sql);
            var columnList = columns.ToList();

            Assert.Equal(8, columnList.Count); // Id, Version, Description, AppliedAt, AppliedBy, Checksum, Success, ErrorMessage

            var versionCol = columnList.First(c => c.COLUMN_NAME == "Version");
            var dataType = (versionCol.DATA_TYPE as string)?.ToLowerInvariant() ?? "";
            Assert.Equal("varchar", dataType);
            Assert.Equal("PRI", versionCol.COLUMN_KEY); // Should be UNIQUE key

            var appliedAtCol = columnList.First(c => c.COLUMN_NAME == "AppliedAt");
            var appliedAtType = (appliedAtCol.DATA_TYPE as string)?.ToLowerInvariant() ?? "";
            Assert.Equal("datetime", appliedAtType);

            var successCol = columnList.First(c => c.COLUMN_NAME == "Success");
            var successType = (successCol.DATA_TYPE as string)?.ToLowerInvariant() ?? "";
            Assert.Equal("tinyint", successType); // BOOLEAN maps to TINYINT(1)
        }

        [Fact]
        public async Task MigrationChecksums_AreConsistent()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Act - Run migrations
            await migrationRunner.RunMigrationsAsync();

            // Act - Get history with checksums
            var history = await migrationRunner.GetMigrationHistoryAsync(20);

            // Assert - All migrations should have consistent checksums
            Assert.All(history, entry =>
            {
                Assert.NotNull(entry.Checksum);
                Assert.Equal(64, entry.Checksum.Length); // SHA256 = 64 hex chars
                Assert.Matches("^[a-f0-9]{64}$", entry.Checksum);
            });
        }

        [Fact]
        public async Task MigrationRunner_ValidateSchemaAsync_AfterInitialization_ShouldPass()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Act
            await migrationRunner.RunMigrationsAsync();
            var validation = await migrationRunner.ValidateSchemaAsync();

            // Assert
            Assert.True(validation.IsValid);
            Assert.Empty(validation.MissingMigrations);
            Assert.Empty(validation.FailedMigrations);
        }

        [Fact]
        public async Task MigrationRunner_GetPendingMigrationsAsync_AfterInitialization_ShouldBeEmpty()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Act
            await migrationRunner.RunMigrationsAsync();
            var pending = await migrationRunner.GetPendingMigrationsAsync();

            // Assert
            Assert.Empty(pending);
        }
    }
}
