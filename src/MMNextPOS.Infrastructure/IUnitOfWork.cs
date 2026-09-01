using System;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;

namespace MMNextPOS.Infrastructure
{
    /// <summary>
    /// Unit of Work abstraction for MySQL transactions.
    /// Provides a single transaction scope across multiple repositories.
    /// </summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// The underlying MySQL connection (shared across repositories in this unit of work).
        /// </summary>
        MySqlConnection Connection { get; }

        /// <summary>
        /// The active transaction (null if not started).
        /// </summary>
        MySqlTransaction? Transaction { get; }

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}