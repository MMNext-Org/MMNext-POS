using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Creates stock movement records (header + detail lines) inside the
    /// active <see cref="MMNextPOS.Infrastructure.IUnitOfWork"/> transaction.
    /// Used by sales, purchases, returns, and adjustments to keep an
    /// auditable history of every quantity change.
    /// </summary>
    public interface IStockMovementService
    {
        /// <summary>
        /// Persist a stock movement header and one detail row per sale line.
        /// Returns the created movement header with its assigned Id.
        /// </summary>
        Task<StockMovement> AddSaleMovementAsync(
            Sale sale,
            IReadOnlyList<SaleDetail> details,
            int? createdByUserId,
            CancellationToken cancellationToken = default);
    }
}
