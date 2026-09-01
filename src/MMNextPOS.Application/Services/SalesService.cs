using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class SalesService : ISalesService
    {
        private readonly ISaleRepository _saleRepo;
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;

        public SalesService(ISaleRepository saleRepo, IProductRepository productRepo, IUnitOfWork unitOfWork)
        {
            _saleRepo = saleRepo ?? throw new ArgumentNullException(nameof(saleRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Sale> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details, CancellationToken cancellationToken = default)
        {
            // Begin transaction for atomic operation
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Validate stock for each detail
                foreach (var d in details)
                {
                    var product = await _productRepo.GetByIdAsync(d.ProductId, cancellationToken);
                    if (product == null)
                        throw new ValidationException($"Product {d.ProductId} not found.");
                    if (product.StockQuantity < d.Quantity)
                        throw new InsufficientStockException($"Insufficient stock for product {product.Name}.");
                }

                // Create sale with details in the transaction
                var createdSale = await _saleRepo.CreateSaleWithDetailsAsync(sale, details, cancellationToken);

                // Update stock for each detail (within same transaction)
                foreach (var d in details)
                {
                    var product = await _productRepo.GetByIdAsync(d.ProductId, cancellationToken);
                    if (product == null)
                        throw new ValidationException($"Product {d.ProductId} not found after sale creation.");
                    
                    product.StockQuantity -= d.Quantity;
                    await _productRepo.UpdateAsync(product, cancellationToken);
                }

                // Commit the transaction
                await _unitOfWork.CommitAsync(cancellationToken);

                return createdSale;
            }
            catch
            {
                // Rollback on any error
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public Task<IReadOnlyList<Sale>> GetRecentSalesAsync(int count = 20, CancellationToken cancellationToken = default)
        {
            return _saleRepo.GetRecentAsync(count, cancellationToken);
        }

        public Task<SaleDetail> AddSaleDetailAsync(int saleId, SaleDetail detail, CancellationToken cancellationToken = default)
        {
            // Implementation could reuse CreateSaleAsync logic for a single detail.
            throw new NotImplementedException();
        }
    }
}
