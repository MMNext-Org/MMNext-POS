using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

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

        // Lazy-initialized repositories
        private ISaleRepository? _sales;
        private ISaleDetailRepository? _saleDetails;
        private IPurchaseRepository? _purchases;
        private IPurchaseDetailRepository? _purchaseDetails;
        private ICustomerRepository? _customers;
        private ISupplierRepository? _suppliers;
        private IProductRepository? _products;
        private ISalesReturnRepository? _salesReturns;
        private ISalesReturnDetailRepository? _salesReturnDetails;
        private IPurchaseReturnRepository? _purchaseReturns;
        private IPurchaseReturnDetailRepository? _purchaseReturnDetails;
        private ICustomerOutstandingRepository? _customerOutstandings;
        private ISupplierOutstandingRepository? _supplierOutstandings;

        public MySqlUnitOfWork(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));
            }

            // Schema migrations use MySQL user variables (e.g. SET @sql = ...) for idempotent
            // ALTER statements, which require the AllowUserVariables option to be enabled.
            var builder = new MySqlConnectionStringBuilder(connectionString);
            if (!builder.AllowUserVariables)
            {
                builder.AllowUserVariables = true;
            }

            _connection = new MySqlConnection(builder.ConnectionString);
        }

        public MySqlConnection Connection => _connection;

        public MySqlTransaction? Transaction => _transaction;

        // Repository properties (lazy initialization)
        public ISaleRepository Sales => _sales ??= new SaleRepository(this);
        public ISaleDetailRepository SaleDetails => _saleDetails ??= new SaleDetailRepository(this);
        public IPurchaseRepository Purchases => _purchases ??= new PurchaseRepository(this);
        public IPurchaseDetailRepository PurchaseDetails => _purchaseDetails ??= new PurchaseDetailRepository(this);
        public ICustomerRepository Customers => _customers ??= new CustomerRepository(this);
        public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(this);
        public IProductRepository Products => _products ??= new ProductRepository(this);
        public ISalesReturnRepository SalesReturns => _salesReturns ??= new SalesReturnRepository(this);
        public ISalesReturnDetailRepository SalesReturnDetails => _salesReturnDetails ??= new SalesReturnDetailRepository(this);
        public IPurchaseReturnRepository PurchaseReturns => _purchaseReturns ??= new PurchaseReturnRepository(this);
        public IPurchaseReturnDetailRepository PurchaseReturnDetails => _purchaseReturnDetails ??= new PurchaseReturnDetailRepository(this);
        public ICustomerOutstandingRepository CustomerOutstandings => _customerOutstandings ??= new CustomerOutstandingRepository(this);
        public ISupplierOutstandingRepository SupplierOutstandings => _supplierOutstandings ??= new SupplierOutstandingRepository(this);

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

            // Update all repositories with the new transaction
            UpdateRepositoriesTransaction();
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

            // Update repositories to remove transaction (they'll use auto-commit)
            UpdateRepositoriesTransaction();
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

            // Update repositories to remove transaction
            UpdateRepositoriesTransaction();
        }

        private void UpdateRepositoriesTransaction()
        {
            // Repositories are recreated on next access via lazy initialization
            // They will pick up the new transaction (or null) from the UoW
            _sales = null;
            _saleDetails = null;
            _purchases = null;
            _purchaseDetails = null;
            _customers = null;
            _suppliers = null;
            _products = null;
            _salesReturns = null;
            _salesReturnDetails = null;
            _purchaseReturns = null;
            _purchaseReturnDetails = null;
            _customerOutstandings = null;
            _supplierOutstandings = null;
        }

        // Specialized query methods
        public async Task<Sale?> GetSaleWithDetailsAsync(int saleId, CancellationToken cancellationToken = default)
        {
            var sale = await Sales.GetByIdAsync(saleId, cancellationToken).ConfigureAwait(false);
            if (sale == null) return null;

            var details = await SaleDetails.GetAllAsync(cancellationToken).ConfigureAwait(false);
            sale.SaleDetails = details.Where(d => d.SaleId == saleId).ToList();
            return sale;
        }

        public async Task<Purchase?> GetPurchaseWithDetailsAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            var purchase = await Purchases.GetByIdAsync(purchaseId, cancellationToken).ConfigureAwait(false);
            if (purchase == null) return null;

            var details = await PurchaseDetails.GetAllAsync(cancellationToken).ConfigureAwait(false);
            purchase.PurchaseDetails = details.Where(d => d.PurchaseId == purchaseId).ToList();
            return purchase;
        }

        public async Task<PurchaseReturn?> GetPurchaseReturnByIdAsync(int returnId, CancellationToken cancellationToken = default)
        {
            var purchaseReturn = await PurchaseReturns.GetByIdAsync(returnId, cancellationToken).ConfigureAwait(false);
            if (purchaseReturn == null) return null;

            var details = await PurchaseReturnDetails.GetAllAsync(cancellationToken).ConfigureAwait(false);
            purchaseReturn.PurchaseReturnDetails = details.Where(d => d.PurchaseReturnId == returnId).ToList();
            return purchaseReturn;
        }

        public async Task<CustomerOutstanding?> GetCustomerOutstandingByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            var all = await CustomerOutstandings.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.FirstOrDefault(o => o.CustomerId == customerId);
        }

        public async Task<SupplierOutstanding?> GetSupplierOutstandingBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            var all = await SupplierOutstandings.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.FirstOrDefault(o => o.SupplierId == supplierId);
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
