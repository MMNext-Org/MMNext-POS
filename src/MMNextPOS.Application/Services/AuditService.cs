using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IChangeDateLogRepository _auditRepo;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuditService(IChangeDateLogRepository auditRepo)
        {
            _auditRepo = auditRepo ?? throw new ArgumentNullException(nameof(auditRepo));
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task LogAsync(string entityName, int entityId, string action, object? oldValues, object? newValues,
            int? userId = null, string? userName = null, string? description = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var log = new ChangeDateLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, _jsonOptions) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues, _jsonOptions) : null,
                    ChangedByUserId = userId,
                    ChangedByUserName = userName,
                    Description = description,
                    IpAddress = GetLocalIpAddress()
                };

                await _auditRepo.AddAsync(log, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Swallow audit logging errors to not break the main operation
                // In production, you might want to log this to a separate logging system
            }
        }

        private static string GetLocalIpAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "Unknown";
        }
    }
}
