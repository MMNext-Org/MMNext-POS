using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISalesService
    {
        Task<Sale> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Sale>> GetRecentSalesAsync(int count = 20, CancellationToken cancellationToken = default);
        Task<SaleDetail> AddSaleDetailAsync(int saleId, SaleDetail detail, CancellationToken cancellationToken = default);
        Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all sales with optional filtering.
        /// </summary>
        /// <param name="fromDate">Filter sales from this date (inclusive).</param>
        /// <param name="toDate">Filter sales to this date (inclusive).</param>
        /// <param name="customerId">Filter by customer ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Filtered list of sales.</returns>
        Task<IReadOnlyList<Sale>> GetAllAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? customerId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the sale details for a specific sale.
        /// </summary>
        Task<IReadOnlyList<SaleDetail>> GetSaleDetailsAsync(int saleId, CancellationToken cancellationToken = default);
    }
}
