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
    }
}
