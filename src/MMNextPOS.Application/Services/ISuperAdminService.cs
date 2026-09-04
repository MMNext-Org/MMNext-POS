using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISuperAdminService
    {
        Task<SuperAdminLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SuperAdminLog>> GetByUserAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SuperAdminLog>> GetByModuleAsync(string module, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SuperAdminLog>> GetByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SuperAdminLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SuperAdminLog>> GetAllAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task<SuperAdminLog> AddAsync(SuperAdminLog log, CancellationToken cancellationToken = default);
        
        // Security operations
        Task<bool> LockUserAsync(int userId, string reason, int adminUserId, CancellationToken cancellationToken = default);
        Task<bool> UnlockUserAsync(int userId, int adminUserId, CancellationToken cancellationToken = default);
        Task<bool> ResetUserPasswordAsync(int userId, string newPassword, int adminUserId, CancellationToken cancellationToken = default);
        Task<bool> DeactivateUserAsync(int userId, int adminUserId, CancellationToken cancellationToken = default);
        
        // System maintenance
        Task<bool> ClearCacheAsync(CancellationToken cancellationToken = default);
        Task<bool> RebuildIndexesAsync(CancellationToken cancellationToken = default);
        Task<bool> VacuumDatabaseAsync(CancellationToken cancellationToken = default);
        
        // Security audit
        Task<IReadOnlyList<SecurityAuditResult>> GetSecurityAuditAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    }

    public class SecurityAuditResult
    {
        public string Category { get; set; } = string.Empty;
        public string Check { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Pass, Fail, Warning
        public string Details { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Info, Warning, Critical
        public string Recommendation { get; set; } = string.Empty;
    }
}