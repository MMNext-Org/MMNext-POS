using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MMNextPOS.Infrastructure
{
    /// <summary>
    /// Interface for running database schema migrations with version tracking.
    /// </summary>
    public interface IMigrationRunner
    {
        /// <summary>
        /// Gets the current applied schema version.
        /// </summary>
        Task<string?> GetCurrentVersionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all pending migrations that have not been applied yet.
        /// </summary>
        Task<IReadOnlyList<MigrationInfo>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs all pending migrations in order.
        /// </summary>
        Task<MigrationResult> RunMigrationsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs a specific migration by version.
        /// </summary>
        Task<MigrationResult> RunMigrationAsync(string version, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the migration history.
        /// </summary>
        Task<IReadOnlyList<MigrationHistoryEntry>> GetMigrationHistoryAsync(int limit = 100, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates that the database schema matches expected state (no drift).
        /// </summary>
        Task<SchemaValidationResult> ValidateSchemaAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all failed migration versions.
        /// </summary>
        Task<IReadOnlyList<string>> GetFailedMigrationsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Information about a migration script.
    /// </summary>
    public class MigrationInfo
    {
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result of running migrations.
    /// </summary>
    public class MigrationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int MigrationsApplied { get; set; }
        public int MigrationsSkipped { get; set; }
        public int MigrationsFailed { get; set; }
        public List<MigrationStepResult> StepResults { get; set; } = new();
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Result of a single migration step.
    /// </summary>
    public class MigrationStepResult
    {
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// Migration history entry from SchemaVersions table.
    /// </summary>
    public class MigrationHistoryEntry
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public string AppliedBy { get; set; } = string.Empty;
        public string? Checksum { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Result of schema validation.
    /// </summary>
    public class SchemaValidationResult
    {
        public bool IsValid { get; set; }
        public string CurrentVersion { get; set; } = string.Empty;
        public string ExpectedVersion { get; set; } = string.Empty;
        public List<string> DriftDetails { get; set; } = new();
        public List<string> MissingMigrations { get; set; } = new();
        public List<string> FailedMigrations { get; set; } = new();
    }
}
