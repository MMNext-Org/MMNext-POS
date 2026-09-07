using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Products")
        {
        }

        public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default, int? minStockAlertLevel = null)
        {
            var level = minStockAlertLevel ?? 5;  // Default value if not provided
            const string sql = "SELECT * FROM Products WHERE IsActive = 1 AND StockQuantity <= @MinStockAlertLevel";
            var result = await Connection.QueryAsync<Product>(sql, new { MinStockAlertLevel = level }, Transaction).ConfigureAwait(false);
            return result.AsList();
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

        public async Task<bool> TryDecrementStockAsync(int productId, int quantity, int adjustedBy, string reason, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for a decrement.");
            }

            // Single atomic UPDATE: only succeeds when Id matches AND stock is sufficient.
            // Returns rows-affected so the caller can tell "no such product" from
            // "insufficient stock" by checking the row's existence separately if needed.
            const string sql = @"
UPDATE Products
SET StockQuantity = StockQuantity - @Quantity,
    LastAdjustment = @LastAdjustment,
    AdjustedBy = @AdjustedBy,
    AdjustmentReason = @AdjustmentReason,
    IsActive = 1
WHERE Id = @ProductId AND StockQuantity >= @Quantity";

            var rows = await Connection.ExecuteAsync(
                new CommandDefinition(sql,
                    new
                    {
                        Quantity = quantity,
                        LastAdjustment = DateTime.UtcNow,
                        AdjustedBy = adjustedBy,
                        AdjustmentReason = reason ?? "Stock adjustment",
                        ProductId = productId
                    },
                    transaction: Transaction,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            return rows == 1;
        }

        public async Task<bool> TryIncrementStockAsync(int productId, int quantity, int adjustedBy, string reason, CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for an increment.");
            }

            // Single atomic UPDATE: only succeeds when Id matches. There is no
            // stock-sufficiency check (incrementing is always possible).
            const string sql = @"
UPDATE Products
SET StockQuantity = StockQuantity + @Quantity,
    LastAdjustment = @LastAdjustment,
    AdjustedBy = @AdjustedBy,
    AdjustmentReason = @AdjustmentReason,
    IsActive = 1
WHERE Id = @ProductId";

            var rows = await Connection.ExecuteAsync(
                new CommandDefinition(sql,
                    new
                    {
                        Quantity = quantity,
                        LastAdjustment = DateTime.UtcNow,
                        AdjustedBy = adjustedBy,
                        AdjustmentReason = reason ?? "Stock adjustment",
                        ProductId = productId
                    },
                    transaction: Transaction,
                    cancellationToken: cancellationToken)).ConfigureAwait(false);

            return rows == 1;
        }
    }
}
