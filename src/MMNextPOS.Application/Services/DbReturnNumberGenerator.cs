using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MMNextPOS.Infrastructure;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// MySQL-backed return number generator. Reserves the next sequence value
    /// for the current year using <c>INSERT ... ON DUPLICATE KEY UPDATE
    /// LastValue = LastValue + 1</c>. The active <see cref="IUnitOfWork"/>
    /// transaction holds a row lock on the <c>ReturnSequences</c> row, so two
    /// concurrent returns cannot issue the same number.
    /// </summary>
    public sealed class DbReturnNumberGenerator : IReturnNumberGenerator
    {
        private readonly IUnitOfWork _unitOfWork;

        public DbReturnNumberGenerator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Prefix is required.", nameof(prefix));
            }

            var normalizedPrefix = prefix.ToUpperInvariant();
            var year = DateTime.UtcNow.Year;

            // Two-step atomic reserve inside the caller's transaction.
            // Step 1: ensure the (Year, Prefix) row exists.
            const string ensureSql = @"
INSERT INTO ReturnSequences (Year, Prefix, LastValue)
VALUES (@Year, @Prefix, 0)
ON DUPLICATE KEY UPDATE Prefix = Prefix;";

            await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(ensureSql,
                    new { Year = year, Prefix = normalizedPrefix },
                    transaction: _unitOfWork.Transaction,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            // Step 2: increment and read the new value in the same statement so two
            // concurrent transactions cannot both observe the same pre-increment value.
            const string incrementSql = @"
UPDATE ReturnSequences
SET LastValue = LastValue + 1
WHERE Year = @Year AND Prefix = @Prefix;

SELECT LastValue FROM ReturnSequences WHERE Year = @Year AND Prefix = @Prefix;";

            // The UPDATE and SELECT are not atomic across two statements on their own,
            // but they are protected by the row-level lock MySQL takes for the UPDATE
            // within the transaction. The subsequent SELECT inside the same transaction
            // will see the row in its updated state (MySQL InnoDB REPEATABLE READ
            // reads its own writes through the transaction).
            await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(incrementSql,
                    new { Year = year, Prefix = normalizedPrefix },
                    transaction: _unitOfWork.Transaction,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            var newValue = await _unitOfWork.Connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    "SELECT LastValue FROM ReturnSequences WHERE Year = @Year AND Prefix = @Prefix",
                    new { Year = year, Prefix = normalizedPrefix },
                    transaction: _unitOfWork.Transaction,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            return ReturnNumberFormat.Format(normalizedPrefix, year, newValue);
        }
    }
}
