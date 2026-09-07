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
    }
}
