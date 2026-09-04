using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IBackupService
    {
        Task<BackupSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<BackupSetting?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BackupSetting>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BackupSetting>> GetActiveBackupsAsync(CancellationToken cancellationToken = default);
        Task<BackupSetting> AddAsync(BackupSetting setting, CancellationToken cancellationToken = default);
        Task UpdateAsync(BackupSetting setting, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Backup execution
        Task<bool> RunBackupAsync(int backupSettingId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BackupResult>> GetBackupHistoryAsync(int backupSettingId, int limit = 50, CancellationToken cancellationToken = default);

        // Restore
        Task<bool> RestoreAsync(int backupSettingId, DateTime? restorePoint = null, CancellationToken cancellationToken = default);
    }

    public class BackupResult
    {
        public int BackupSettingId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = string.Empty; // Success, Failed, Running
        public long BackupSizeBytes { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}