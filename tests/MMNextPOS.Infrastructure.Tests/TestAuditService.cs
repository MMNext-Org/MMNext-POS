using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;

namespace MMNextPOS.Infrastructure.Tests
{
    /// <summary>
    /// Test-only <see cref="IAuditService"/> decorator that delegates every call
    /// to the wrapped service, except it throws on the first invocation of
    /// <see cref="LogAsync"/>. Used by integration tests to verify that
    /// <c>SalesService.CreateSaleAsync</c> rolls back the entire transaction
    /// when the audit write fails (R4 fallback plan).
    /// </summary>
    public sealed class ThrowingAuditService : IAuditService
    {
        private readonly IAuditService _inner;
        private readonly System.Exception _exception;

        public ThrowingAuditService(IAuditService inner, System.Exception exception)
        {
            _inner = inner;
            _exception = exception;
        }

        public Task LogAsync(
            string entityName,
            int entityId,
            string action,
            object? oldValues,
            object? newValues,
            int? userId = null,
            string? userName = null,
            string? description = null,
            CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }
}
