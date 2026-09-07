using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default, int? minStockAlertLevel = null);
        Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
        Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task AdjustStockAsync(int productId, int quantityAdjustment, string reason, int adjustedBy, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atomically reserves (decrements) stock for a sale. Returns true if the
        /// product had enough stock and the UPDATE affected one row, false if the
        /// product does not exist or its current stock is below <paramref name="quantity"/>.
        /// The decrement and the existence check are one SQL statement, so two
        /// concurrent sales against the same product cannot both succeed when
        /// only one unit is left.
        /// </summary>
        Task<bool> TryDecrementStockAsync(int productId, int quantity, int adjustedBy, string reason, CancellationToken cancellationToken = default);
    }
}
