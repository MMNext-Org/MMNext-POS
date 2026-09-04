using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;

        public ProductService(IProductRepository productRepo)
        {
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
        }

        public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _productRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _productRepo.GetAllAsync(cancellationToken);
        }

        public Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
        {
            return _productRepo.GetLowStockProductsAsync(cancellationToken);
        }

        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            return _productRepo.AddAsync(product, cancellationToken);
        }

        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            return _productRepo.UpdateAsync(product, cancellationToken);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return _productRepo.DeleteAsync(id, cancellationToken);
        }
    }
}
