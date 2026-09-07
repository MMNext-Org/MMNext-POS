using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MMNextPOS.Infrastructure;

namespace MMNextPOS.Infrastructure
{
    /// <summary>
    /// Ensures that the required database tables exist by running schema migrations.
    /// Called at application startup. Uses MigrationRunner for versioned, idempotent migrations.
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly IMigrationRunner _migrationRunner;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            IMigrationRunner migrationRunner,
            ILogger<DatabaseInitializer> logger)
        {
            _migrationRunner = migrationRunner ?? throw new ArgumentNullException(nameof(migrationRunner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the database by running all pending migrations.
        /// This method is idempotent - running it multiple times will only apply missing migrations.
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("[DatabaseInitializer] Starting database initialization...");
            _logger.LogInformation("Starting database initialization...");

            try
            {
                // Validate current schema state before migrations
                var validationBefore = await _migrationRunner.ValidateSchemaAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"[DatabaseInitializer] Schema validation before: Current={validationBefore.CurrentVersion}, Expected={validationBefore.ExpectedVersion}, Valid={validationBefore.IsValid}");
                _logger.LogInformation("Schema validation before migration: Current={CurrentVersion}, Expected={ExpectedVersion}, Valid={IsValid}",
                    validationBefore.CurrentVersion, validationBefore.ExpectedVersion, validationBefore.IsValid);

                if (!validationBefore.IsValid)
                {
                    _logger.LogWarning("Schema drift detected: {Details}", string.Join("; ", validationBefore.DriftDetails));
                }

                // Run all pending migrations
                Console.WriteLine("[DatabaseInitializer] Calling RunMigrationsAsync...");
                var result = await _migrationRunner.RunMigrationsAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"[DatabaseInitializer] RunMigrationsAsync result: Success={result.Success}, Applied={result.MigrationsApplied}, Skipped={result.MigrationsSkipped}, Failed={result.MigrationsFailed}, Error={result.ErrorMessage}");

                if (!result.Success)
                {
                    var errorMsg = $"Database migration failed: {result.ErrorMessage}";
                    _logger.LogError(errorMsg);
                    throw new InvalidOperationException(errorMsg);
                }

                _logger.LogInformation("Applied {Count} migrations in {Duration}ms",
                    result.MigrationsApplied, result.Duration.TotalMilliseconds);

                // Validate schema after migrations
                var validationAfter = await _migrationRunner.ValidateSchemaAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Schema validation after migration: Current={CurrentVersion}, Expected={ExpectedVersion}, Valid={IsValid}",
                    validationAfter.CurrentVersion, validationAfter.ExpectedVersion, validationAfter.IsValid);

                if (!validationAfter.IsValid)
                {
                    var errorMsg = $"Schema validation failed after migration: {string.Join("; ", validationAfter.DriftDetails)}";
                    _logger.LogError(errorMsg);
                    throw new InvalidOperationException(errorMsg);
                }

                _logger.LogInformation("Database initialization completed successfully. Current version: {Version}", validationAfter.CurrentVersion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }

        /// <summary>
        /// Gets the current schema version without running migrations.
        /// Useful for health checks and diagnostics.
        /// </summary>
        public async Task<string?> GetCurrentVersionAsync(CancellationToken cancellationToken = default)
        {
            return await _migrationRunner.GetCurrentVersionAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets pending migrations without applying them.
        /// Useful for pre-flight checks.
        /// </summary>
        public async Task<IReadOnlyList<MigrationInfo>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
        {
            return await _migrationRunner.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates the current schema state against expected migrations.
        /// </summary>
        public async Task<SchemaValidationResult> ValidateSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await _migrationRunner.ValidateSchemaAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
