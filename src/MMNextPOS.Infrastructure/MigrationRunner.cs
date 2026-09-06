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

            if (!pendingMigrations.Any())
            {
                _logger.LogInformation("No pending migrations to apply");
                result.Success = true;
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
ORDER BY AppliedAt DESC
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
            result.FailedMigrations = failedMigrations;

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
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                .Where(n => n.Contains("Migrations.", StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n)
                .ToList();

            _logger.LogInformation("Found {Count} migration resources: {Names}", resourceNames.Count, string.Join(", ", resourceNames));

            foreach (var resourceName in resourceNames)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                using var reader = new StreamReader(stream, Encoding.UTF8);
                var content = reader.ReadToEnd();

                // Extract version from resource name (e.g., "MMNextPOS.Infrastructure.Migrations.001_InitialSchema.sql")
                // Find "Migrations." and get everything after it
                var migrationsIndex = resourceName.IndexOf("Migrations.", StringComparison.OrdinalIgnoreCase);
                var fileName = migrationsIndex >= 0 
                    ? resourceName.Substring(migrationsIndex + "Migrations.".Length)
                    : resourceName.Substring(resourceName.LastIndexOf('.') + 1);
                
                _logger.LogInformation("Processing migration: resourceName={ResourceName}, fileName={FileName}", resourceName, fileName);
                
                var version = ExtractVersionFromFileName(fileName);

                var checksum = ComputeChecksum(content);

                migrations.Add(new MigrationInfo
                {
                    Version = version,
                    Description = ExtractDescription(content),
                    FileName = fileName,
                    Checksum = checksum
                });
            }

            return migrations;
        }

        private string ExtractVersionFromFileName(string fileName)
        {
            // fileName format: "001_InitialSchema.sql" or "000_BaselineSchemaVersions.sql"
            // Extract version from before the first underscore
            var parts = fileName.Split('_', 2);
            if (parts.Length >= 1 && int.TryParse(parts[0], out _))
            {
                return parts[0];
            }
            return fileName;
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

        private async Task<List<string>> GetFailedMigrationsAsync(CancellationToken cancellationToken)
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
                    foreach (var statement in statements)
                    {
                        if (string.IsNullOrWhiteSpace(statement)) continue;
                        await _unitOfWork.Connection.ExecuteAsync(statement, transaction: _unitOfWork.Transaction).ConfigureAwait(false);
                    }

                    // Record successful migration
                    await RecordMigrationAsync(migration, currentChecksum, true, null, cancellationToken).ConfigureAwait(false);

                    await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

                    stepResult.Success = true;
                    stepResult.Duration = DateTime.UtcNow - stepStartTime;

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

                _logger.LogError(ex, "Migration {Version} failed: {Error}", migration.Version, ex.Message);
            }

            return stepResult;
        }

        private string GetMigrationContent(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                _logger.LogWarning("Migration resource not found: {FileName}", fileName);
                return string.Empty;
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return string.Empty;

            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
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