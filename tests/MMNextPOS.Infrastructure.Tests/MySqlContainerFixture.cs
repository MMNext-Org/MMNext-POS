using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MySql;
using Dapper;
using MMNextPOS.Application;
using Xunit;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// Shared xUnit Collection Fixture that spins up a single MySQL container for the
    /// duration of the test collection. This avoids resource exhaustion from spinning
    /// up many parallel MySQL containers (the heavy migration 007 alone runs 282
    /// statements, so each new container adds significant memory pressure).
    /// </summary>
    public sealed class MySqlContainerFixture : IAsyncLifetime
    {
        public MySqlContainer Container { get; private set; } = null!;
        public IConfiguration Configuration { get; private set; } = null!;
        public IServiceProvider ServiceProvider { get; private set; } = null!;
        public string ConnectionString { get; private set; } = string.Empty;

        public async Task InitializeAsync()
        {
            Container = new MySqlBuilder()
                .WithDatabase("mmnextpos_migration_test")
                .WithUsername("test")
                .WithPassword("test")
                .WithImage("mysql:8.0")
                .WithCleanUp(true)
                .Build();
            await Container.StartAsync();

            ConnectionString = Container.GetConnectionString();
            // MySqlConnector requires "Allow User Variables=true" for SET @var / PREPARE statements
            if (!ConnectionString.Contains("Allow User Variables", StringComparison.OrdinalIgnoreCase))
            {
                ConnectionString += ";Allow User Variables=true";
            }

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = ConnectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));
            services.AddApplication(Configuration);
            ServiceProvider = services.BuildServiceProvider();
        }

        public async Task DisposeAsync()
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            if (Container != null)
            {
                await Container.DisposeAsync();
            }
        }

        /// <summary>
        /// Drops every table in the active database, leaving the schema empty so the
        /// next test can run migrations from scratch. Uses a fresh connection because
        /// MySQL will not let us DROP a schema/tables we are actively using.
        /// </summary>
        public async Task ResetDatabaseAsync()
        {
            await using var conn = new MySqlConnector.MySqlConnection(ConnectionString);
            await conn.OpenAsync();

            const string disableFk = "SET FOREIGN_KEY_CHECKS = 0;";
            const string enableFk = "SET FOREIGN_KEY_CHECKS = 1;";

            await conn.ExecuteAsync(disableFk);

            const string listTables = @"
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE()";

            var tables = await conn.QueryAsync<string>(listTables);
            foreach (var table in tables)
            {
                var safe = table.Replace("`", "``");
                await conn.ExecuteAsync($"DROP TABLE IF EXISTS `{safe}`");
            }

            await conn.ExecuteAsync(enableFk);
        }
    }

    [CollectionDefinition(nameof(MySqlContainerCollection))]
    public sealed class MySqlContainerCollection : ICollectionFixture<MySqlContainerFixture>
    {
    }
}
