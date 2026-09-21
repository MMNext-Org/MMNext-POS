using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Database connection testing service for operational validation.
    /// </summary>
    public interface IConnectionTestService
    {
        Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default);
        Task<ServerInfo> GetServerInfoAsync(CancellationToken cancellationToken = default);
        Task<int> GetTableCountAsync(CancellationToken cancellationToken = default);
        Task<long> GetDatabaseSizeBytesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of a connection test operation.
    /// </summary>
    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public long LatencyMs { get; set; }
        public string? ServerVersion { get; set; }
        public string? ServerVersionText { get; set; }
    }

    /// <summary>
    /// Database server information.
    /// </summary>
    public class ServerInfo
    {
        public string Version { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public System.DateTime ServerTime { get; set; }
        public long SizeBytes { get; set; }
    }
}
