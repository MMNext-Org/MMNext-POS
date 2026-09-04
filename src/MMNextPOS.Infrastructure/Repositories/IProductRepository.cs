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
    }
}