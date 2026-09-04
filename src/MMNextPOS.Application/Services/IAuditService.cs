using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IAuditService
    {
        Task LogAsync(string entityName, int entityId, string action, object? oldValues, object? newValues,
            int? userId = null, string? userName = null, string? description = null, CancellationToken cancellationToken = default);
    }
}
