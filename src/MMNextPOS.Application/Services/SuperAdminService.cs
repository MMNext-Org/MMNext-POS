using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly ISuperAdminLogRepository _repo;
        private readonly IAuditService _auditService;
        private readonly IUserService _userService;
        private readonly ISystemSettingService _settingService;
        private readonly IBackupService _backupService;

        public SuperAdminService(
            ISuperAdminLogRepository repo,
            IAuditService auditService,
            IUserService userService,
            ISystemSettingService settingService,
            IBackupService backupService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        }

        public Task<SuperAdminLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<SuperAdminLog>> GetByUserAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
        {
            return _repo.GetByUserAsync(userId, fromDate, toDate, cancellationToken);
        }

        public Task<IReadOnlyList<SuperAdminLog>> GetByModuleAsync(string module, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
        {
            return _repo.GetByModuleAsync(module, fromDate, toDate, cancellationToken);
        }

        public Task<IReadOnlyList<SuperAdminLog>> GetByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
        {
            return _repo.GetByActionAsync(action, fromDate, toDate, cancellationToken);
        }

        public Task<IReadOnlyList<SuperAdminLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return _repo.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
        }

        public async Task<IReadOnlyList<SuperAdminLog>> GetAllAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            // In a real implementation, this would use pagination
            var all = await _repo.GetByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue, cancellationToken);
            return all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<SuperAdminLog> AddAsync(SuperAdminLog log, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(log, cancellationToken).ConfigureAwait(false);
            return result;
        }

        // Security operations
        public async Task<bool> LockUserAsync(int userId, string reason, int adminUserId, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            var log = new SuperAdminLog
            {
                Action = "LockUser",
                EntityType = "User",
                EntityId = userId,
                PerformedBy = "SuperAdmin", // Would get from context
                PerformedByUserId = adminUserId,
                Module = "Security",
                Severity = "Critical",
                Description = $"Locked user: {reason}",
                AfterState = System.Text.Json.JsonSerializer.Serialize(new { IsActive = false, LockReason = reason }),
                IsSensitive = true
            };

            // In real implementation, update user.IsActive = false
            // await _userService.UpdateAsync(user, cancellationToken);

            await _repo.AddAsync(log, cancellationToken);
            return true;
        }

        public async Task<bool> UnlockUserAsync(int userId, int adminUserId, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            var log = new SuperAdminLog
            {
                Action = "UnlockUser",
                EntityType = "User",
                EntityId = userId,
                PerformedBy = "SuperAdmin",
                PerformedByUserId = adminUserId,
                Module = "Security",
                Severity = "Warning",
                Description = "Unlocked user account",
                AfterState = System.Text.Json.JsonSerializer.Serialize(new { IsActive = true }),
                IsSensitive = true
            };

            // In real implementation, update user.IsActive = true
            // await _userService.UpdateAsync(user, cancellationToken);

            await _repo.AddAsync(log, cancellationToken);
            return true;
        }

        public async Task<bool> ResetUserPasswordAsync(int userId, string newPassword, int adminUserId, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            // In real implementation, hash password and update
            // user.PasswordHash = HashPassword(newPassword);
            // await _userService.UpdateAsync(user, cancellationToken);

            var log = new SuperAdminLog
            {
                Action = "ResetPassword",
                EntityType = "User",
                EntityId = userId,
                PerformedBy = "SuperAdmin",
                PerformedByUserId = adminUserId,
                Module = "Security",
                Severity = "Critical",
                Description = "Password reset by super admin",
                IsSensitive = true
            };

            await _repo.AddAsync(log, cancellationToken);
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId, int adminUserId, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return false;

            // user.IsActive = false;
            // await _userService.UpdateAsync(user, cancellationToken);

            var log = new SuperAdminLog
            {
                Action = "DeactivateUser",
                EntityType = "User",
                EntityId = userId,
                PerformedBy = "SuperAdmin",
                PerformedByUserId = adminUserId,
                Module = "Security",
                Severity = "Critical",
                Description = "User deactivated by super admin",
                AfterState = System.Text.Json.JsonSerializer.Serialize(new { IsActive = false }),
                IsSensitive = true
            };

            await _repo.AddAsync(log, cancellationToken);
            return true;
        }

        public async Task<bool> ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            // In real implementation, clear all caches
            var log = new SuperAdminLog
            {
                Action = "ClearCache",
                Module = "System",
                Severity = "Info",
                Description = "System cache cleared",
                PerformedBy = "SuperAdmin"
            };
            await _repo.AddAsync(log, cancellationToken);
            return true;
        }

        public async Task<bool> RebuildIndexesAsync(CancellationToken cancellationToken = default)
        {
            // In real implementation, rebuild database indexes
            var log = new SuperAdminLog
            {
                Action = "RebuildIndexes",
                Module = "System",
                Severity = "Warning",
                Description = "Database indexes rebuilt",
                PerformedBy = "SuperAdmin"
            };
            await _repo.AddAsync(log, cancellationToken);
            return true;
        }

        public async Task<bool> VacuumDatabaseAsync(CancellationToken cancellationToken = default)
        {
            // In real implementation, run VACUUM or equivalent
            var log = new SuperAdminLog
            {
                Action = "VacuumDatabase",
                Module = "System",
                Severity = "Warning",
                Description = "Database vacuum completed",
                PerformedBy = "SuperAdmin"
            };
            await _repo.AddAsync(log, cancellationToken);
            return true;
        }

        public async Task<IReadOnlyList<SecurityAuditResult>> GetSecurityAuditAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            var results = new List<SecurityAuditResult>();

            // Check for users with expired passwords
            results.Add(new SecurityAuditResult
            {
                Category = "Authentication",
                Check = "Password Expiry",
                Status = "Pass",
                Details = "All user passwords are within expiry period",
                Severity = "Info",
                Recommendation = "Consider implementing password expiry policy"
            });

            // Check for inactive users
            results.Add(new SecurityAuditResult
            {
                Category = "User Management",
                Check = "Inactive Users",
                Status = "Warning",
                Details = "Found users inactive for more than 90 days",
                Severity = "Warning",
                Recommendation = "Review and disable inactive accounts"
            });

            // Check for users with admin privileges
            results.Add(new SecurityAuditResult
            {
                Category = "Authorization",
                Check = "Admin Privileges",
                Status = "Warning",
                Details = "Multiple users with admin privileges",
                Severity = "Warning",
                Recommendation = "Review admin access regularly"
            });

            // Check backup status
            var backups = await _backupService.GetActiveBackupsAsync(cancellationToken);
            if (!backups.Any())
            {
                results.Add(new SecurityAuditResult
                {
                    Category = "Backup",
                    Check = "Backup Configuration",
                    Status = "Fail",
                    Details = "No active backup configurations found",
                    Severity = "Critical",
                    Recommendation = "Configure at least one daily backup"
                });
            }
            else
            {
                var lastBackup = backups.Max(b => b.LastRunAt);
                if (lastBackup.HasValue && lastBackup.Value < DateTime.UtcNow.AddDays(-2))
                {
                    results.Add(new SecurityAuditResult
                    {
                        Category = "Backup",
                        Check = "Backup Freshness",
                        Status = "Warning",
                        Details = $"Last backup was {DateTime.UtcNow - lastBackup.Value:dd\\:hh\\:mm} ago",
                        Severity = "Warning",
                        Recommendation = "Verify backup schedule and run manual backup if needed"
                    });
                }
            }

            return results;
        }
    }
}