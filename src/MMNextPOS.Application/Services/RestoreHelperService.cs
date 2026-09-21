using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Database restore helpers for operational recovery.
    /// </summary>
    public static class RestoreHelperService
    {
        /// <summary>
        /// Verifies that a backup file exists and is readable.
        /// </summary>
        public static Task<RestoreValidationResult> ValidateBackupFileAsync(string backupPath, CancellationToken cancellationToken = default)
        {
            var result = new RestoreValidationResult();
            try
            {
                if (string.IsNullOrWhiteSpace(backupPath))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Backup path is null or empty.";
                    return Task.FromResult(result);
                }

                if (!File.Exists(backupPath))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Backup file does not exist: {backupPath}";
                    return Task.FromResult(result);
                }

                var fileInfo = new FileInfo(backupPath);
                result.FileSizeBytes = fileInfo.Length;
                result.IsValid = true;
                result.FilePath = Path.GetFullPath(backupPath);
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Error validating backup file: {ex.Message}";
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Reads the database.sql script content from a backup ZIP file.
        /// </summary>
        public static async Task<string?> ReadBackupSqlFromZipAsync(string zipPath, CancellationToken cancellationToken = default)
        {
            using var archive = ZipFile.OpenRead(zipPath);
            var entry = archive.GetEntry("database.sql");
            if (entry == null) return null;

            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        /// <summary>
        /// Executes a MySQL script against the target database asynchronously.
        /// This is designed for test/repair scenarios and should cluster transactional batches.
        /// SAFETY: In production, this should ALWAYS be gated by restore confirmation + backup verification.
        /// </summary>
        public static async Task<RestoreResult> ExecuteSqlScriptAsync(
            string connectionString,
            string script,
            int? maxStatementsPerBatch = 10,
            CancellationToken cancellationToken = default)
        {
            var result = new RestoreResult();
            var statements = SplitSqlStatements(script).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            result.TotalStatements = statements.Count;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                var batch = new List<string>();

                foreach (var stmt in statements)
                {
                    batch.Add(stmt);

                    if (batch.Count >= maxStatementsPerBatch || stmt == statements.Last())
                    {
                        if (batch.Any())
                        {
                            var batchSql = string.Join(";", batch);
                            await ExecuteBatchAsync(connection, batchSql, cancellationToken).ConfigureAwait(false);
                            result.SuccessCount++;
                            batch.Clear();
                        }
                    }
                }

                result.IsSuccess = true;
                result.Message = $"Restored {result.SuccessCount} statements successfully.";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Restore failed: {ex.Message}";
                sw.Stop();
                result.DurationMs = sw.ElapsedMilliseconds;
            }

            return result;
        }

        private static List<string> SplitSqlStatements(string sql)
        {
            var statements = new List<string>();
            var sb = new StringBuilder();

            var lines = sql.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("--")) continue;

                sb.AppendLine(line);
                var upper = trimmed.ToUpperInvariant();
                if (upper.EndsWith(";") || upper == ";")
                {
                    var fullStatement = sb.ToString().Trim();
                    if (fullStatement.EndsWith(";"))
                        fullStatement = fullStatement.Substring(0, fullStatement.Length - 1);
                    if (!string.IsNullOrWhiteSpace(fullStatement))
                        statements.Add(fullStatement);
                    sb.Clear();
                }
            }

            if (sb.Length > 0)
            {
                var remaining = sb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(remaining))
                    statements.Add(remaining);
            }

            return statements;
        }

        private static async Task ExecuteBatchAsync(MySqlConnection connection, string sql, CancellationToken cancellationToken)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 600;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Result of a restore operation.
    /// </summary>
    public class RestoreResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public long DurationMs { get; set; }
        public int SuccessCount { get; set; }
        public int TotalStatements { get; set; }
    }

    /// <summary>
    /// Result of validating a backup file.
    /// </summary>
    public class RestoreValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
        public long FileSizeBytes { get; set; }
    }
}
