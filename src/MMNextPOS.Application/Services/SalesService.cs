using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.Application.Services;

namespace MMNextPOS.Application.Services
{
    public class SalesService : ISalesService
    {
        private readonly ISaleRepository _saleRepo;
        private readonly ISaleDetailRepository _saleDetailRepo;
        private readonly IProductRepository _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public SalesService(ISaleRepository saleRepo, ISaleDetailRepository saleDetailRepo, IProductRepository productRepo, IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _saleRepo = saleRepo ?? throw new ArgumentNullException(nameof(saleRepo));
            _saleDetailRepo = saleDetailRepo ?? throw new ArgumentNullException(nameof(saleDetailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
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

                // Audit log: sale created
                await _auditService.LogAsync(
                    entityName: nameof(Sale),
                    entityId: createdSale.Id,
                    action: "Create",
                    oldValues: null,
                    newValues: createdSale,
                    userId: null,
                    userName: null,
                    description: $"Sale created with {details.Count()} details, total {createdSale.TotalAmount:C2}",
                    cancellationToken: cancellationToken);

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

        public async Task<SaleDetail> AddSaleDetailAsync(int saleId, SaleDetail detail, CancellationToken cancellationToken = default)
        {
            // Create a minimal sale with the single detail (walk-in customer, current date).
            var minimalSale = new Sale
            {
                CustomerId = 0, // walk-in / unspecified
                SaleDate = DateTime.UtcNow,
                TotalAmount = detail.UnitPrice * detail.Quantity
            };

            var details = new List<SaleDetail> { detail };

            // Validate stock (same logic as CreateSaleAsync)
            var product = await _productRepo.GetByIdAsync(detail.ProductId, cancellationToken);
            if (product == null)
                throw new ValidationException($"Product {detail.ProductId} not found.");
            if (product.StockQuantity < detail.Quantity)
                throw new InsufficientStockException($"Insufficient stock for product {product.Name}.");

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create sale with details
                var createdSale = await _saleRepo.CreateSaleWithDetailsAsync(minimalSale, details, cancellationToken);

                // Update stock
                product.StockQuantity -= detail.Quantity;
                await _productRepo.UpdateAsync(product, cancellationToken);

                // Commit
                await _unitOfWork.CommitAsync(cancellationToken);

                // Audit log
                await _auditService.LogAsync(
                    entityName: nameof(Sale),
                    entityId: createdSale.Id,
                    action: "Create",
                    oldValues: null,
                    newValues: createdSale,
                    userId: null,
                    userName: null,
                    description: $"Sale detail added: {detail.ProductId}, Qty={detail.Quantity}",
                    cancellationToken: cancellationToken);

                return detail;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _saleRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return GetAllAsync(null, null, null, null, null, cancellationToken);
        }

        public async Task<IReadOnlyList<Sale>> GetAllAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? customerId = null,
            string? status = null,
            int? locationId = null,
            CancellationToken cancellationToken = default)
        {
            var allSales = await _saleRepo.GetAllAsync(cancellationToken);

            var filtered = allSales.AsQueryable();

            if (fromDate.HasValue)
            {
                filtered = filtered.Where(s => s.SaleDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // "To this date (inclusive)": date-only callers expect the whole day included.
                filtered = filtered.Where(s => s.SaleDate < toDate.Value.Date.AddDays(1));
            }

            if (customerId.HasValue)
            {
                filtered = filtered.Where(s => s.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filtered = filtered.Where(s => s.Status != null && s.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (locationId.HasValue)
            {
                filtered = filtered.Where(s => s.LocationId == locationId.Value);
            }

            return filtered.OrderByDescending(s => s.SaleDate).ToList();
        }

        public async Task<IReadOnlyList<SaleDetail>> GetSaleDetailsAsync(int saleId, CancellationToken cancellationToken = default)
        {
            return await _saleDetailRepo.GetBySaleIdAsync(saleId, cancellationToken);
        }
    }
}
