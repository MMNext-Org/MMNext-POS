using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Writes StockMovement + StockMovementDetail rows in the active
    /// transaction. The caller (e.g. <see cref="SalesService"/>) controls
    /// the Unit of Work boundary.
    /// </summary>
    public sealed class StockMovementService : IStockMovementService
    {
        private readonly IStockMovementRepository _movementRepo;
        private readonly IStockMovementDetailRepository _detailRepo;

        public StockMovementService(
            IStockMovementRepository movementRepo,
            IStockMovementDetailRepository detailRepo)
        {
            _movementRepo = movementRepo ?? throw new ArgumentNullException(nameof(movementRepo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
        }

        public async Task<StockMovement> AddSaleMovementAsync(
            Sale sale,
            IReadOnlyList<SaleDetail> details,
            int? createdByUserId,
            CancellationToken cancellationToken = default)
        {
            if (sale == null) throw new ArgumentNullException(nameof(sale));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var totalQty = details.Sum(d => d.Quantity);
            var movementNo = $"SM-{sale.SaleDate:yyyyMMddHHmmss}-{sale.Id}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = "Sale",
                MovementDate = sale.SaleDate == default ? DateTime.UtcNow : sale.SaleDate,
                LocationId = sale.LocationId,
                CustomerId = sale.CustomerId > 0 ? sale.CustomerId : (int?)null,
                Quantity = totalQty,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                Reason = $"Sale #{sale.Id}",
                IsActive = true
            };

            var created = await _movementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            foreach (var d in details)
            {
                var detailRow = new StockMovementDetail
                {
                    StockMovementId = created.Id,
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitCost = d.UnitPrice, // cost layer work is scheduled in Phase 4
                    LineTotal = d.Quantity * d.UnitPrice,
                    Notes = d.Id == 0 ? null : $"SaleDetail {d.Id}"
                };
                await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);
            }

            return created;
        }

        public async Task<StockMovement> AddPurchaseMovementAsync(
            Purchase purchase,
            IReadOnlyList<PurchaseDetail> details,
            int? createdByUserId,
            CancellationToken cancellationToken = default)
        {
            if (purchase == null) throw new ArgumentNullException(nameof(purchase));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var totalQty = details.Sum(d => d.Quantity);
            var movementNo = $"SM-{purchase.PurchaseDate:yyyyMMddHHmmss}-P-{purchase.Id}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = "Purchase",
                MovementDate = purchase.PurchaseDate == default ? DateTime.UtcNow : purchase.PurchaseDate,
                LocationId = purchase.LocationId,
                SupplierId = purchase.SupplierId > 0 ? purchase.SupplierId : (int?)null,
                Quantity = totalQty,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                Reason = $"Purchase #{purchase.Id} ({purchase.InvoiceNo})",
                IsActive = true
            };

            var created = await _movementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            foreach (var d in details)
            {
                var detailRow = new StockMovementDetail
                {
                    StockMovementId = created.Id,
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitCost = d.UnitPrice,
                    LineTotal = d.Quantity * d.UnitPrice,
                    Notes = d.Notes ?? (d.Id == 0 ? null : $"PurchaseDetail {d.Id}")
                };
                await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);
            }

            return created;
        }

        public async Task<StockMovement> AddReturnMovementAsync(
            int returnId,
            int productId,
            int quantity,
            decimal unitCost,
            int? locationId,
            int? createdByUserId,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for a return movement.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");

            var movementNo = $"SM-RET-{DateTime.UtcNow:yyyyMMddHHmmss}-R-{returnId}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = "Return",
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                Reason = reason ?? $"Return #{returnId}",
                IsActive = true
            };

            var created = await _movementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            var detailRow = new StockMovementDetail
            {
                StockMovementId = created.Id,
                ProductId = productId,
                Quantity = quantity,
                UnitCost = unitCost,
                LineTotal = quantity * unitCost,
                Notes = $"ReturnDetail for Return #{returnId}"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddVoidMovementAsync(
            int saleId,
            int productId,
            int quantity,
            decimal unitCost,
            int? locationId,
            int? createdByUserId,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            var movementNo = $"SM-VOID-{DateTime.UtcNow:yyyyMMddHHmmss}-S-{saleId}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = "Void",
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                Reason = reason ?? $"Void Sale #{saleId}",
                IsActive = true
            };

            var created = await _movementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            // Only add detail row if we have a specific product
            if (productId > 0 && quantity > 0)
            {
                var detailRow = new StockMovementDetail
                {
                    StockMovementId = created.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitCost = unitCost,
                    LineTotal = quantity * unitCost,
                    Notes = $"VoidDetail for Sale #{saleId}"
                };
                await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);
            }

            return created;
        }
    }
}
