using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IMigrationService
    {
        Task<DataMigration?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<DataMigration?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DataMigration>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DataMigration>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DataMigration>> GetRunningMigrationsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DataMigration>> GetScheduledMigrationsAsync(CancellationToken cancellationToken = default);
        Task<DataMigration> AddAsync(DataMigration migration, CancellationToken cancellationToken = default);
        Task UpdateAsync(DataMigration migration, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Migration execution
        Task<bool> RunMigrationAsync(int migrationId, CancellationToken cancellationToken = default);
        Task<bool> RunMigrationAsync(string migrationName, CancellationToken cancellationToken = default);
        Task<bool> CancelMigrationAsync(int migrationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MigrationStepResult>> GetMigrationHistoryAsync(int migrationId, int limit = 50, CancellationToken cancellationToken = default);
    }

    public class MigrationStepResult
    {
        public int MigrationId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Pending, Running, Completed, Failed, Skipped
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsFailed { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}