using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MMNextPOS.Infrastructure
{
    /// <summary>
    /// Runs database schema migrations with version tracking and idempotence guarantees.
    /// Migrations are embedded SQL files executed in version order.
    /// </summary>
    public sealed class MigrationRunner : IMigrationRunner
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MigrationRunner> _logger;
        private readonly ConnectionStringOptions _connectionOptions;
        private readonly List<MigrationInfo> _migrations;

        public MigrationRunner(
            IUnitOfWork unitOfWork,
            ILogger<MigrationRunner> logger,
            IOptions<ConnectionStringOptions> connectionOptions)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionOptions = connectionOptions?.Value ?? throw new ArgumentNullException(nameof(connectionOptions));

            _migrations = LoadMigrations();
        }

        /// <inheritdoc />
        public async Task<string?> GetCurrentVersionAsync(CancellationToken cancellationToken = default)
        {
            await EnsureSchemaVersionsTableAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"
SELECT Version
FROM SchemaVersions
WHERE Success = 1
ORDER BY AppliedAt DESC
LIMIT 1";

            return await _unitOfWork.Connection
                .QuerySingleOrDefaultAsync<string>(sql, transaction: _unitOfWork.Transaction)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MigrationInfo>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
        {
            await EnsureSchemaVersionsTableAsync(cancellationToken).ConfigureAwait(false);

            var appliedVersions = await GetAppliedVersionsAsync(cancellationToken).ConfigureAwait(false);
            var appliedSet = new HashSet<string>(appliedVersions, StringComparer.OrdinalIgnoreCase);

            return _migrations
                .Where(m => !appliedSet.Contains(m.Version))
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc />
        public async Task<MigrationResult> RunMigrationsAsync(CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var result = new MigrationResult();
            var pendingMigrations = await GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
            var appliedVersions = await GetAppliedVersionsAsync(cancellationToken).ConfigureAwait(false);

            Console.WriteLine($"[MigrationRunner] RunMigrationsAsync: found {pendingMigrations.Count} pending migrations, {appliedVersions.Count} already applied");
            foreach (var m in pendingMigrations)
            {
                Console.WriteLine($"[MigrationRunner] Pending: {m.Version} - {m.Description}");
            }

            if (!pendingMigrations.Any())
            {
                _logger.LogInformation("No pending migrations to apply. {Count} migrations already applied.", appliedVersions.Count);
                result.Success = true;
                result.MigrationsSkipped = appliedVersions.Count;
                result.Duration = DateTime.UtcNow - startTime;
                return result;
            }

            _logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count);

            foreach (var migration in pendingMigrations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stepResult = await RunSingleMigrationAsync(migration, cancellationToken).ConfigureAwait(false);
                result.StepResults.Add(stepResult);

                if (stepResult.Success)
                {
                    result.MigrationsApplied++;
                }
                else
                {
                    result.MigrationsFailed++;
                    result.Success = false;
                    result.ErrorMessage = stepResult.ErrorMessage;
                    break;
                }
            }

            result.Duration = DateTime.UtcNow - startTime;
            result.Success = result.MigrationsFailed == 0;

            _logger.LogInformation(
                "Migration run completed: {Applied} applied, {Skipped} skipped, {Failed} failed in {Duration}ms",
                result.MigrationsApplied, result.MigrationsSkipped, result.MigrationsFailed, result.Duration.TotalMilliseconds);

            return result;
        }

        /// <inheritdoc />
        public async Task<MigrationResult> RunMigrationAsync(string version, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var result = new MigrationResult();

            var migration = _migrations.FirstOrDefault(m => m.Version.Equals(version, StringComparison.OrdinalIgnoreCase));
            if (migration == null)
            {
                result.Success = false;
                result.ErrorMessage = $"Migration version {version} not found";
                result.Duration = DateTime.UtcNow - startTime;
                return result;
            }

            var appliedVersions = await GetAppliedVersionsAsync(cancellationToken).ConfigureAwait(false);
            if (appliedVersions.Contains(version, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Migration {Version} already applied, skipping", version);
                result.Success = true;
                result.MigrationsSkipped = 1;
                result.Duration = DateTime.UtcNow - startTime;
                return result;
            }

            var stepResult = await RunSingleMigrationAsync(migration, cancellationToken).ConfigureAwait(false);
            result.StepResults.Add(stepResult);

            if (stepResult.Success)
            {
                result.MigrationsApplied = 1;
                result.Success = true;
            }
            else
            {
                result.MigrationsFailed = 1;
                result.Success = false;
                result.ErrorMessage = stepResult.ErrorMessage;
            }

            result.Duration = DateTime.UtcNow - startTime;
            return result;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<MigrationHistoryEntry>> GetMigrationHistoryAsync(int limit = 100, CancellationToken cancellationToken = default)
        {
            await EnsureSchemaVersionsTableAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"
SELECT Id, Version, Description, AppliedAt, AppliedBy, Checksum, Success, ErrorMessage
FROM SchemaVersions
ORDER BY AppliedAt DESC, Id DESC
LIMIT @Limit";

            var result = await _unitOfWork.Connection
                .QueryAsync<MigrationHistoryEntry>(sql, new { Limit = limit }, _unitOfWork.Transaction)
                .ConfigureAwait(false);

            return result.AsList();
        }

        /// <inheritdoc />
        public async Task<SchemaValidationResult> ValidateSchemaAsync(CancellationToken cancellationToken = default)
        {
            var result = new SchemaValidationResult
            {
                CurrentVersion = await GetCurrentVersionAsync(cancellationToken) ?? "0.0.0",
                ExpectedVersion = _migrations.LastOrDefault()?.Version ?? "0.0.0"
            };

            var pendingMigrations = await GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
            result.MissingMigrations = pendingMigrations.Select(m => m.Version).ToList();

            var failedMigrations = await GetFailedMigrationsAsync(cancellationToken).ConfigureAwait(false);
            result.FailedMigrations = failedMigrations.ToList();

            result.IsValid = !result.MissingMigrations.Any() && !result.FailedMigrations.Any();

            if (!result.IsValid)
            {
                result.DriftDetails.Add($"Current version: {result.CurrentVersion}");
                result.DriftDetails.Add($"Expected version: {result.ExpectedVersion}");
                if (result.MissingMigrations.Any())
                {
                    result.DriftDetails.Add($"Missing migrations: {string.Join(", ", result.MissingMigrations)}");
                }
                if (result.FailedMigrations.Any())
                {
                    result.DriftDetails.Add($"Failed migrations: {string.Join(", ", result.FailedMigrations)}");
                }
            }

            return result;
        }

        private List<MigrationInfo> LoadMigrations()
        {
            var migrations = new List<MigrationInfo>();
            var assembly = Assembly.GetExecutingAssembly();

            // Debug: List all manifest resources
            var allResources = assembly.GetManifestResourceNames();
            Console.WriteLine($"[MigrationRunner] All manifest resources: {string.Join(", ", allResources)}");
            _logger.LogInformation("All manifest resources: {Resources}", string.Join(", ", allResources));

            // Hardcode the known migrations to avoid resource name parsing issues
            var knownMigrations = new[]
            {
                new { Version = "000", FileName = "000_BaselineSchemaVersions.sql", Description = "Create SchemaVersions table for tracking applied migrations" },
                new { Version = "001", FileName = "001_InitialSchema.sql", Description = "Initial Schema - All Core Tables" },
                new { Version = "002", FileName = "002_SeedDefaultData.sql", Description = "Seed Default Data" },
                new { Version = "003", FileName = "003_AddSalesColumns.sql", Description = "Add Missing Columns to Sales Table" },
                new { Version = "004", FileName = "004_AddIndexes.sql", Description = "Add Performance Indexes" },
                new { Version = "005", FileName = "005_MissingEntityTables.sql", Description = "Add Missing Entity Tables (Registrations, RemoteWarehouses, Subscriptions, DashboardWidgets)" },
                new { Version = "006", FileName = "006_AlignSchemaWithEntities.sql", Description = "Align Schema With Entity Models (audit fields, column gaps, model-mismatched table rebuilds)" },
                new { Version = "007", FileName = "007_AddMissingFKs.sql", Description = "Add Missing Foreign Keys for Audit Fields" }
            };

            foreach (var known in knownMigrations)
            {
                var resourceName = $"MMNextPOS.Infrastructure.Migrations.{known.FileName}";

                Console.WriteLine($"[MigrationRunner] Looking for resource: {resourceName}");
                _logger.LogInformation("Looking for resource: {ResourceName}", resourceName);
                var exists = allResources.Any(r => r.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"[MigrationRunner] Resource exists: {exists}");
                _logger.LogInformation("Resource exists: {Exists}", exists);

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Console.WriteLine($"[MigrationRunner] ERROR: Migration resource not found: {resourceName}");
                    _logger.LogError("Migration resource not found: {ResourceName}", resourceName);
                    continue;
                }

                using var reader = new StreamReader(stream, Encoding.UTF8);
                var content = reader.ReadToEnd();

                var checksum = ComputeChecksum(content);

                migrations.Add(new MigrationInfo
                {
                    Version = known.Version,
                    Description = known.Description,
                    FileName = known.FileName,
                    ResourceName = resourceName,
                    Checksum = checksum
                });
            }

            Console.WriteLine($"[MigrationRunner] Loaded {migrations.Count} migrations: {string.Join(", ", migrations.Select(m => m.Version))}");
            _logger.LogInformation("Loaded {Count} migrations: {Versions}", migrations.Count, string.Join(", ", migrations.Select(m => m.Version)));
            return migrations;
        }

        private string ExtractDescription(string sqlContent)
        {
            // Look for comment at the top: -- Description: ...
            var lines = sqlContent.Split('\n');
            foreach (var line in lines.Take(10))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("-- Description:", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring("-- Description:".Length).Trim();
                }
                if (trimmed.StartsWith("-- Migration", StringComparison.OrdinalIgnoreCase) && trimmed.Contains(":"))
                {
                    var colonIndex = trimmed.IndexOf(':');
                    if (colonIndex >= 0)
                    {
                        return trimmed.Substring(colonIndex + 1).Trim();
                    }
                }
            }
            return "No description";
        }

        private string ComputeChecksum(string content)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private async Task EnsureSchemaVersionsTableAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS SchemaVersions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Version VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(500) NOT NULL,
    AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    AppliedBy VARCHAR(100) NOT NULL,
    Checksum VARCHAR(64) NULL,
    Success BOOLEAN DEFAULT 1,
    ErrorMessage VARCHAR(2000) NULL,
    INDEX IX_SchemaVersions_Version (Version),
    INDEX IX_SchemaVersions_AppliedAt (AppliedAt)
) ENGINE=InnoDB;";

            await _unitOfWork.Connection.ExecuteAsync(sql, transaction: _unitOfWork.Transaction).ConfigureAwait(false);
        }

        private async Task<List<string>> GetAppliedVersionsAsync(CancellationToken cancellationToken)
        {
            const string sql = "SELECT Version FROM SchemaVersions WHERE Success = 1";
            var result = await _unitOfWork.Connection
                .QueryAsync<string>(sql, transaction: _unitOfWork.Transaction)
                .ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<string>> GetFailedMigrationsAsync(CancellationToken cancellationToken)
        {
            const string sql = "SELECT Version FROM SchemaVersions WHERE Success = 0";
            var result = await _unitOfWork.Connection
                .QueryAsync<string>(sql, transaction: _unitOfWork.Transaction)
                .ConfigureAwait(false);
            return result.AsList();
        }

        private async Task<MigrationStepResult> RunSingleMigrationAsync(MigrationInfo migration, CancellationToken cancellationToken)
        {
            var stepStartTime = DateTime.UtcNow;
            var stepResult = new MigrationStepResult
            {
                Version = migration.Version,
                Description = migration.Description
            };

            Console.WriteLine($"[MigrationRunner] Applying migration {migration.Version}: {migration.Description}");
            _logger.LogInformation("Applying migration {Version}: {Description}", migration.Version, migration.Description);

            try
            {
                // Get migration content from embedded resource
                var content = GetMigrationContent(migration.FileName);
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new InvalidOperationException($"Migration content not found for {migration.FileName}");
                }

                // Verify checksum
                var currentChecksum = ComputeChecksum(content);
                if (!string.IsNullOrEmpty(migration.Checksum) && !migration.Checksum.Equals(currentChecksum, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Migration checksum mismatch for {migration.Version}. Expected: {migration.Checksum}, Got: {currentChecksum}");
                }

                // Execute migration in a transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    // Split SQL into individual statements and execute
                    var statements = SplitSqlStatements(content);
                    Console.WriteLine($"[MigrationRunner] Migration {migration.Version}: executing {statements.Count} statements");
                    foreach (var statement in statements)
                    {
                        if (string.IsNullOrWhiteSpace(statement)) continue;
                        Console.WriteLine($"[MigrationRunner] Executing: {statement.Substring(0, Math.Min(100, statement.Length))}...");
                        await _unitOfWork.Connection.ExecuteAsync(statement, transaction: _unitOfWork.Transaction).ConfigureAwait(false);
                    }

                    // Record successful migration
                    await RecordMigrationAsync(migration, currentChecksum, true, null, cancellationToken).ConfigureAwait(false);

                    await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

                    stepResult.Success = true;
                    stepResult.Duration = DateTime.UtcNow - stepStartTime;

                    Console.WriteLine($"[MigrationRunner] Migration {migration.Version} applied successfully in {stepResult.Duration.TotalMilliseconds}ms");
                    _logger.LogInformation("Migration {Version} applied successfully in {Duration}ms", migration.Version, stepResult.Duration.TotalMilliseconds);
                }
                catch
                {
                    await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    throw;
                }
            }
            catch (Exception ex)
            {
                stepResult.Success = false;
                stepResult.ErrorMessage = ex.Message;
                stepResult.Duration = DateTime.UtcNow - stepStartTime;

                // Record failed migration
                try
                {
                    await RecordMigrationAsync(migration, migration.Checksum, false, ex.Message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception recordEx)
                {
                    _logger.LogError(recordEx, "Failed to record migration failure for {Version}", migration.Version);
                }

                Console.WriteLine($"[MigrationRunner] Migration {migration.Version} failed: {ex.Message}");
                _logger.LogError(ex, "Migration {Version} failed: {Error}", migration.Version, ex.Message);
            }

            return stepResult;
        }

        private string GetMigrationContent(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // First try to find by exact resource name (stored in MigrationInfo.ResourceName)
            // We need to find the migration by fileName first to get its ResourceName
            var migration = _migrations.FirstOrDefault(m => m.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (migration != null && !string.IsNullOrEmpty(migration.ResourceName))
            {
                using var stream = assembly.GetManifestResourceStream(migration.ResourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }

            // Fallback: try to find by EndsWith
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                _logger.LogWarning("Migration resource not found: {FileName}", fileName);
                return string.Empty;
            }

            using var stream2 = assembly.GetManifestResourceStream(resourceName);
            if (stream2 == null) return string.Empty;

            using var reader2 = new StreamReader(stream2, Encoding.UTF8);
            return reader2.ReadToEnd();
        }

        private List<string> SplitSqlStatements(string sql)
        {
            var statements = new List<string>();
            var currentStatement = new StringBuilder();
            var delimiter = ";";

            using var reader = new StringReader(sql);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmed = line.Trim();

                // Skip comments
                if (trimmed.StartsWith("--") || trimmed.StartsWith("#"))
                {
                    continue;
                }

                // Handle DELIMITER changes
                if (trimmed.StartsWith("DELIMITER", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = trimmed.Split(' ', 2);
                    if (parts.Length > 1)
                    {
                        delimiter = parts[1].Trim();
                    }
                    continue;
                }

                currentStatement.AppendLine(line);

                // Check for statement terminator
                if (trimmed.EndsWith(delimiter))
                {
                    var stmt = currentStatement.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(stmt))
                    {
                        // Remove the delimiter
                        if (stmt.EndsWith(delimiter))
                        {
                            stmt = stmt.Substring(0, stmt.Length - delimiter.Length).Trim();
                        }
                        statements.Add(stmt);
                    }
                    currentStatement.Clear();
                }
            }

            // Add any remaining statement
            var remaining = currentStatement.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(remaining))
            {
                statements.Add(remaining);
            }

            return statements;
        }

        private async Task RecordMigrationAsync(
            MigrationInfo migration,
            string checksum,
            bool success,
            string? errorMessage,
            CancellationToken cancellationToken)
        {
            const string sql = @"
INSERT INTO SchemaVersions (Version, Description, AppliedAt, AppliedBy, Checksum, Success, ErrorMessage)
VALUES (@Version, @Description, @AppliedAt, @AppliedBy, @Checksum, @Success, @ErrorMessage)
ON DUPLICATE KEY UPDATE
    Description = @Description,
    AppliedAt = @AppliedAt,
    AppliedBy = @AppliedBy,
    Checksum = @Checksum,
    Success = @Success,
    ErrorMessage = @ErrorMessage";

            await _unitOfWork.Connection.ExecuteAsync(sql, new
            {
                Version = migration.Version,
                Description = migration.Description,
                AppliedAt = DateTime.UtcNow,
                AppliedBy = "MigrationRunner",
                Checksum = checksum,
                Success = success,
                ErrorMessage = errorMessage ?? string.Empty
            }, _unitOfWork.Transaction).ConfigureAwait(false);
        }
    }
}
