using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Connection test service implementation for validating database connections.
    /// </summary>
    public class ConnectionTestService : IConnectionTestService
    {
        private readonly string _connectionString;
        private readonly IAuditService _auditService;

        public ConnectionTestService(string connectionString, IAuditService auditService)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();

                await _auditService.LogAsync("ConnectionTest", 0, "Test", null,
                    new { Result = "Success", LatencyMs = stopwatch.ElapsedMilliseconds },
                    null, "System",
                    $"Connection test passed. Latency: {stopwatch.ElapsedMilliseconds}ms",
                    cancellationToken).ConfigureAwait(false);

                return new ConnectionTestResult
                {
                    Success = true,
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    ServerVersion = connection.ServerVersion
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _auditService.LogAsync("ConnectionTest", 0, "Test", null,
                    new { Result = "Failed", Error = ex.Message },
                    null, "System",
                    $"Connection test failed: {ex.Message}",
                    cancellationToken).ConfigureAwait(false);

                return new ConnectionTestResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    LatencyMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        public async Task<ServerInfo> GetServerInfoAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var tableCount = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE()",
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            return new ServerInfo
            {
                Version = connection.ServerVersion ?? "Unknown",
                Database = connection.Database,
                TableCount = tableCount,
                ServerTime = DateTime.UtcNow
            };
        }

        public async Task<int> GetTableCountAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var result = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE()",
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
            return result;
        }

        public async Task<long> GetDatabaseSizeBytesAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var result = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    "SELECT SUM(data_length + index_length) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE()",
                    cancellationToken: cancellationToken)).ConfigureAwait(false);
            return result;
        }
    }
}
