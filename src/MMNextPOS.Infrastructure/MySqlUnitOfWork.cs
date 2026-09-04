using System;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace MMNextPOS.Infrastructure
{
    /// <summary>
    /// MySQL implementation of IUnitOfWork using MySqlTransaction.
    /// Manages a single connection and transaction lifetime.
    /// </summary>
    public sealed class MySqlUnitOfWork : IUnitOfWork
    {
        private readonly MySqlConnection _connection;
        private MySqlTransaction? _transaction;
        private bool _disposed;

        public MySqlUnitOfWork(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public MySqlConnection Connection => _connection;

        public MySqlTransaction? Transaction => _transaction;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction already started. Call CommitAsync or RollbackAsync first.");
            }

            if (_connection.State != System.Data.ConnectionState.Open)
            {
                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            _transaction = await _connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                return; // No-op if no transaction
            }

            await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync().ConfigureAwait(false);
                }
                if (_connection != null)
                {
                    await _connection.DisposeAsync().ConfigureAwait(false);
                }
                _disposed = true;
            }
        }
    }
}
