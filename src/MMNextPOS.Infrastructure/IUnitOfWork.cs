using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

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

        // Repository properties
        ISaleRepository Sales { get; }
        ISaleDetailRepository SaleDetails { get; }
        IPurchaseRepository Purchases { get; }
        IPurchaseDetailRepository PurchaseDetails { get; }
        ICustomerRepository Customers { get; }
        ISupplierRepository Suppliers { get; }
        IProductRepository Products { get; }
        ISalesReturnRepository SalesReturns { get; }
        ISalesReturnDetailRepository SalesReturnDetails { get; }
        IPurchaseReturnRepository PurchaseReturns { get; }
        IPurchaseReturnDetailRepository PurchaseReturnDetails { get; }
        ICustomerOutstandingRepository CustomerOutstandings { get; }
        ISupplierOutstandingRepository SupplierOutstandings { get; }

        // Specialized query methods
        Task<Sale?> GetSaleWithDetailsAsync(int saleId, CancellationToken cancellationToken = default);
        Task<Purchase?> GetPurchaseWithDetailsAsync(int purchaseId, CancellationToken cancellationToken = default);
        Task<PurchaseReturn?> GetPurchaseReturnByIdAsync(int returnId, CancellationToken cancellationToken = default);
        Task<CustomerOutstanding?> GetCustomerOutstandingByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
        Task<SupplierOutstanding?> GetSupplierOutstandingBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default);
    }
}
