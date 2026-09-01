using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBase, IProductRepository
    {
        public ProductRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            const string sql = @"INSERT INTO Products (Sku, Name, Price, StockQuantity) VALUES (@Sku, @Name, @Price, @StockQuantity);
                                 SELECT LAST_INSERT_ID();";
            var id = await Connection.ExecuteScalarAsync<long>(sql, product, Transaction).ConfigureAwait(false);
            product.Id = (int)id;
            return product;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "DELETE FROM Products WHERE Id = @Id";
            await Connection.ExecuteAsync(sql, new { Id = id }, Transaction).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM Products";
            var result = await Connection.QueryAsync<Product>(sql, transaction: Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default, int? minStockAlertLevel = null)
        {
            var level = minStockAlertLevel ?? 5;  // Default value if not provided
            const string sql = "SELECT * FROM Products WHERE IsActive = 1 AND StockQuantity <= @MinStockAlertLevel";
            var result = await Connection.QueryAsync<Product>(sql, new { MinStockAlertLevel = level }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM Products WHERE Id = @Id";
            return await Connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id }, Transaction).ConfigureAwait(false);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            const string sql = "UPDATE Products SET Sku = @Sku, Name = @Name, Price = @Price, StockQuantity = @StockQuantity WHERE Id = @Id";
            await Connection.ExecuteAsync(sql, product, Transaction).ConfigureAwait(false);
        }

        public async Task AdjustStockAsync(int productId, int quantityAdjustment, string reason, int adjustedBy, CancellationToken cancellationToken = default)
        {
            // Get current product
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null)
                throw new KeyNotFoundException($"Product {productId} not found");

            // Calculate new stock quantity
            int newStock = product.StockQuantity + quantityAdjustment;
            if (newStock < 0)
                throw new InvalidOperationException("Stock cannot go below zero");

            // Update product stock in one operation
            var updateSql = @"
                UPDATE Products 
                SET StockQuantity = @NewStock,
                    LastAdjustment = @LastAdjustment,
                    AdjustedBy = @AdjustedBy,
                    AdjustmentReason = @AdjustmentReason,
                    IsActive = 1
                WHERE Id = @ProductId";

            await Connection.ExecuteAsync(updateSql, new
            {
                NewStock = newStock,
                LastAdjustment = DateTime.UtcNow,
                AdjustedBy = adjustedBy,
                AdjustmentReason = reason ?? "Stock adjustment",
                ProductId = productId
            }, Transaction).ConfigureAwait(false);
        }
    }
}
