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
        /// <param name="status">Filter by sale status (e.g., "Completed", "Hold", "Voided").</param>
        /// <param name="locationId">Filter by location ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Filtered list of sales.</returns>
        Task<IReadOnlyList<Sale>> GetAllAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? customerId = null,
            string? status = null,
            int? locationId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the sale details for a specific sale.
        /// </summary>
        Task<IReadOnlyList<SaleDetail>> GetSaleDetailsAsync(int saleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes a sales return/refund.
        /// </summary>
        /// <param name="returnRequest">The return request containing sale ID, items to return, and reason.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created sales return.</returns>
        Task<SalesReturn> ProcessReturnAsync(SalesReturnRequest returnRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Voids a completed sale, restoring stock and reversing customer outstanding.
        /// </summary>
        /// <param name="saleId">The ID of the sale to void.</param>
        /// <param name="reason">Reason for voiding the sale.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The voided sale.</returns>
        Task<Sale> VoidSaleAsync(int saleId, string reason, CancellationToken cancellationToken = default);

        /// <summary>
        /// Aggregates duplicate lines for the same product, summing quantities, discount amounts, and tax amounts.
        /// Validates that discount and tax amounts are non-negative.
        /// </summary>
        /// <param name="details">The sale details to aggregate.</param>
        /// <exception cref="ValidationException">Thrown if any DiscountAmount or TaxAmount is negative.</exception>
        Task AggregateDuplicateLines(IEnumerable<SaleDetail> details);

        /// <summary>
        /// Rounds UnitPrice, DiscountAmount, TaxAmount using MidpointRounding.ToEven (banker's rounding),
        /// then recomputes LineTotal = Quantity * UnitPrice - DiscountAmount + TaxAmount, rounded to 2dp.
        /// </summary>
        /// <param name="details">The sale details to round.</param>
        Task RoundLines(IEnumerable<SaleDetail> details);
    }

    /// <summary>
    /// Request object for processing a sales return.
    /// </summary>
    public class SalesReturnRequest
    {
        public string? ReturnNo { get; set; }
        public int SaleId { get; set; }
        public int CustomerId { get; set; }
        public string? Reason { get; set; }
        public List<SalesReturnLineRequest> Lines { get; set; } = new();
        public int? CreatedByUserId { get; set; }
        public string? RefundMethod { get; set; } // Cash, Card, StoreCredit
    }

    /// <summary>
    /// Individual line item for a sales return request.
    /// </summary>
    public class SalesReturnLineRequest
    {
        public int SaleDetailId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Reason { get; set; }
    }
}