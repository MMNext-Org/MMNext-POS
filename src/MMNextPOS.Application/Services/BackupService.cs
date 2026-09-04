using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class BackupService : IBackupService
    {
        private readonly IBackupSettingRepository _repo;
        private readonly IAuditService _auditService;

        public BackupService(
            IBackupSettingRepository repo,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<BackupSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public Task<BackupSetting?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return _repo.GetByNameAsync(name, cancellationToken);
        }

        public Task<IReadOnlyList<BackupSetting>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public Task<IReadOnlyList<BackupSetting>> GetActiveBackupsAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetActiveBackupsAsync(cancellationToken);
        }

        public async Task<BackupSetting> AddAsync(BackupSetting setting, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(setting, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(BackupSetting), result.Id, "Create", null, result, 1, "System", $"Created backup setting {result.Name}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(BackupSetting setting, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(setting.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(setting, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(BackupSetting), setting.Id, "Update", existing, setting, 1, "System", $"Updated backup setting {setting.Name}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(BackupSetting), id, "Delete", existing, null, 1, "System", $"Deleted backup setting {existing?.Name ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        // Backup execution
        public async Task<bool> RunBackupAsync(int backupSettingId, CancellationToken cancellationToken = default)
        {
            var setting = await _repo.GetByIdAsync(backupSettingId, cancellationToken);
            if (setting == null)
            {
                return false;
            }

            var result = new BackupResult
            {
                BackupSettingId = setting.Id,
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };

            try
            {
                var backupPath = await CreateBackupAsync(setting, cancellationToken);
                
                result.CompletedAt = DateTime.UtcNow;
                result.Status = "Success";
                result.FilePath = backupPath;
                result.BackupSizeBytes = new FileInfo(backupPath).Length;

                // Update setting
                setting.LastRunAt = result.StartedAt;
                setting.NextRunAt = CalculateNextRun(setting);
                setting.LastStatus = "Success";
                setting.LastErrorMessage = string.Empty;
                await _repo.UpdateAsync(setting, cancellationToken).ConfigureAwait(false);

                await _auditService.LogAsync(nameof(BackupSetting), setting.Id, "Backup", null, result, 1, "System", $"Backup completed successfully: {backupPath}", cancellationToken).ConfigureAwait(false);

                // Cleanup old backups
                await CleanupOldBackupsAsync(setting, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                result.CompletedAt = DateTime.UtcNow;
                result.Status = "Failed";
                result.ErrorMessage = ex.Message;

                // Update setting with error
                var settingToUpdate = await _repo.GetByIdAsync(backupSettingId, cancellationToken);
                if (settingToUpdate != null)
                {
                    settingToUpdate.LastStatus = "Failed";
                    settingToUpdate.LastErrorMessage = ex.Message;
                    await _repo.UpdateAsync(settingToUpdate, cancellationToken).ConfigureAwait(false);
                }

                await _auditService.LogAsync(nameof(BackupSetting), backupSettingId, "Backup", null, result, 1, "System", $"Backup failed: {ex.Message}", cancellationToken).ConfigureAwait(false);
                return false;
            }
        }

        public Task<IReadOnlyList<BackupResult>> GetBackupHistoryAsync(int backupSettingId, int limit = 50, CancellationToken cancellationToken = default)
        {
            // In a real implementation, this would query a backup history table
            // For now, return empty list
            return Task.FromResult<IReadOnlyList<BackupResult>>(new List<BackupResult>());
        }

        // Restore
        public async Task<bool> RestoreAsync(int backupSettingId, DateTime? restorePoint = null, CancellationToken cancellationToken = default)
        {
            var setting = await _repo.GetByIdAsync(backupSettingId, cancellationToken);
            if (setting == null)
            {
                return false;
            }

            // In a real implementation, this would restore from a backup file
            // For now, return true as placeholder
            await Task.CompletedTask;
            return true;
        }

        private async Task<string> CreateBackupAsync(BackupSetting setting, CancellationToken cancellationToken)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"MMNextPOS_Backup_{setting.Name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            
            var backupDir = Path.GetDirectoryName(setting.BackupPath) ?? Path.GetTempPath();
            var backupPath = Path.Combine(backupDir, fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath) ?? backupDir);

            using var zipArchive = ZipFile.Open(backupPath, ZipArchiveMode.Create);

            // Backup database - placeholder implementation
            var databaseEntry = zipArchive.CreateEntry("database.sql");
            using (var entryStream = databaseEntry.Open())
            using (var writer = new StreamWriter(entryStream))
            {
                await writer.WriteLineAsync("-- Backup created at " + DateTime.UtcNow);
                await writer.WriteLineAsync("-- This is a placeholder for database dump");
            }

            // Backup files if enabled
            if (setting.IncludeFiles)
            {
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var file in Directory.GetFiles(appDirectory, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = file.Substring(appDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
                    if (!ShouldExcludeFile(relativePath))
                    {
                        var entry = zipArchive.CreateEntry(relativePath);
                        using (var entryStream = entry.Open())
                        using (var fileStream = File.OpenRead(file))
                        {
                            await fileStream.CopyToAsync(entryStream, cancellationToken);
                        }
                    }
                }
            }

            return backupPath;
        }

        private async Task CleanupOldBackupsAsync(BackupSetting setting, CancellationToken cancellationToken)
        {
            var backupDir = Path.GetDirectoryName(setting.BackupPath) ?? Path.GetTempPath();
            var prefix = setting.Name;
            var files = Directory.GetFiles(backupDir, $"{prefix}*.zip")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(setting.RetentionDays)
                .ToList();

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignore delete errors
                }
            }
        }

        private DateTime CalculateNextRun(BackupSetting setting)
        {
            var now = DateTime.UtcNow;
            var today = DateTime.Today;
            var runTime = setting.ExecutionTime;

            DateTime nextRun;
            switch (setting.Frequency)
            {
                case "Daily":
                    nextRun = today.Add(runTime);
                    if (nextRun <= DateTime.Now) nextRun = nextRun.AddDays(1);
                    break;
                case "Weekly":
                    nextRun = today.AddDays(((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7).Add(runTime);
                    if (nextRun <= DateTime.Now) nextRun = nextRun.AddDays(7);
                    break;
                case "Monthly":
                    nextRun = new DateTime(today.Year, today.Month, 1).Add(runTime);
                    if (nextRun <= DateTime.Now) nextRun = nextRun.AddMonths(1);
                    break;
                default:
                    nextRun = DateTime.MaxValue;
                    break;
            }

            return nextRun;
        }

        private bool ShouldExcludeFile(string relativePath)
        {
            var excludePatterns = new[]
            {
                "bin/", "obj/", ".git/", ".vs/", "packages/", "node_modules/",
                "*.log", "*.tmp", "*.bak", "*.old", "*.cache",
                "appsettings.Development.json", "appsettings.Local.json"
            };

            return excludePatterns.Any(p => relativePath.Contains(p.Replace("/", Path.DirectorySeparatorChar.ToString())));
        }
    }
}