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
        private readonly IStockMovementService _stockMovementService;
        private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
        private readonly IInvoiceService _invoiceService;
        private readonly IOutstandingService _outstandingService;

        public SalesService(
            ISaleRepository saleRepo,
            ISaleDetailRepository saleDetailRepo,
            IProductRepository productRepo,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IStockMovementService stockMovementService,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IInvoiceService invoiceService,
            IOutstandingService outstandingService)
        {
            _saleRepo = saleRepo ?? throw new ArgumentNullException(nameof(saleRepo));
            _saleDetailRepo = saleDetailRepo ?? throw new ArgumentNullException(nameof(saleDetailRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _invoiceNumberGenerator = invoiceNumberGenerator ?? throw new ArgumentNullException(nameof(invoiceNumberGenerator));
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _outstandingService = outstandingService ?? throw new ArgumentNullException(nameof(outstandingService));
        }

        public async Task<Sale> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details, CancellationToken cancellationToken = default)
        {
            if (sale == null) throw new ArgumentNullException(nameof(sale));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var detailList = details as IList<SaleDetail> ?? details.ToList();
            if (detailList.Count == 0)
            {
                throw new ValidationException("A sale must have at least one line item.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // 1) Aggregate duplicate lines (same ProductId → sum Quantity, last UnitPrice wins).
                var aggregated = AggregateDuplicateLines(detailList);

                // 2) Atomic stock reservation per line. InsufficientStockException
                //    on any line rolls the whole transaction back; no later lines run.
                foreach (var d in aggregated)
                {
                    var ok = await _productRepo.TryDecrementStockAsync(d.ProductId, d.Quantity, 1, "Sale", cancellationToken).ConfigureAwait(false);
                    if (!ok)
                    {
                        var p = await _productRepo.GetByIdAsync(d.ProductId, cancellationToken).ConfigureAwait(false);
                        if (p == null)
                        {
                            throw new ValidationException($"Product {d.ProductId} not found.");
                        }
                        throw new InsufficientStockException($"Insufficient stock for product {p.Name}.");
                    }
                }

                // 3) Round money per line (banker's half-even) and sum for the header.
                var rounded = RoundLines(aggregated);
                sale.TotalAmount = rounded.Sum(l => l.UnitPrice * l.Quantity);
                if (sale.SaleDate == default)
                {
                    sale.SaleDate = DateTime.UtcNow;
                }
                if (string.IsNullOrWhiteSpace(sale.Status))
                {
                    sale.Status = "Completed";
                }

                // 4) Persist sale header + details.
                var createdSale = await _saleRepo.CreateSaleWithDetailsAsync(sale, rounded, cancellationToken).ConfigureAwait(false);

                // 5) Stock movement header + one detail per sale line.
                await _stockMovementService.AddSaleMovementAsync(createdSale, (IReadOnlyList<SaleDetail>)rounded, createdByUserId: null, cancellationToken).ConfigureAwait(false);

                // 6) Auto-generated invoice number + Invoice row, linked back to the sale.
                var invoiceNo = await _invoiceNumberGenerator.NextAsync("INV", cancellationToken).ConfigureAwait(false);
                var invoice = new Invoice
                {
                    InvoiceNo = invoiceNo,
                    SaleId = createdSale.Id,
                    CustomerId = createdSale.CustomerId,
                    InvoiceDate = createdSale.SaleDate,
                    AmountDue = createdSale.TotalAmount,
                    Status = "Active"
                };
                await _invoiceService.AddAsync(invoice, cancellationToken).ConfigureAwait(false);
                // The Sale entity does not currently carry InvoiceId; the link is
                // preserved via the Invoice row's SaleId FK. Tests verify the
                // Invoice.SaleId, not a back-reference on Sale.

                // 7) Customer outstanding for non-cash sales (CustomerId set).
                if (createdSale.CustomerId > 0 && createdSale.TotalAmount > 0m)
                {
                    var open = new CustomerOutstanding
                    {
                        CustomerId = createdSale.CustomerId,
                        SaleId = createdSale.Id,
                        TransactionDate = createdSale.SaleDate,
                        DebitAmount = createdSale.TotalAmount,
                        CreditAmount = 0m,
                        Balance = createdSale.TotalAmount,
                        Description = $"Auto from sale {createdSale.Id} / invoice {invoiceNo}",
                        Status = "Open"
                    };
                    await _outstandingService.AddCustomerOutstandingAsync(open, cancellationToken).ConfigureAwait(false);
                }

                // 8) Audit INSIDE the transaction. A failure here rolls back the sale,
                //    so the audit row and the business write are atomic.
                await _auditService.LogAsync(
                    entityName: nameof(Sale),
                    entityId: createdSale.Id,
                    action: "Create",
                    oldValues: null,
                    newValues: new
                    {
                        SaleId = createdSale.Id,
                        InvoiceNo = invoiceNo,
                        CustomerId = createdSale.CustomerId,
                        TotalAmount = createdSale.TotalAmount,
                        LineCount = rounded.Count
                    },
                    userId: null,
                    userName: null,
                    description: $"Sale {invoiceNo} posted: {rounded.Count} lines, total {createdSale.TotalAmount:C2}",
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return createdSale;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        public Task<IReadOnlyList<Sale>> GetRecentSalesAsync(int count = 20, CancellationToken cancellationToken = default)
        {
            return _saleRepo.GetRecentAsync(count, cancellationToken);
        }

        public async Task<SaleDetail> AddSaleDetailAsync(int saleId, SaleDetail detail, CancellationToken cancellationToken = default)
        {
            // Single-detail convenience that routes through the same hardened path.
            var sale = new Sale
            {
                Id = 0,
                CustomerId = 0,
                SaleDate = DateTime.UtcNow,
                TotalAmount = detail.UnitPrice * detail.Quantity,
                Status = "Completed"
            };

            var created = await CreateSaleAsync(sale, new[] { detail }, cancellationToken).ConfigureAwait(false);

            detail.SaleId = created.Id;
            return detail;
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

        /// <summary>
        /// Combines repeated lines for the same product into one line. The last-seen
        /// UnitPrice wins (most recent pricing context for the cashier), and quantities
        /// are summed. Throws if any line has a non-positive quantity.
        /// </summary>
        internal static IList<SaleDetail> AggregateDuplicateLines(IList<SaleDetail> input)
        {
            var byProduct = new Dictionary<int, SaleDetail>();
            var order = new List<int>();
            foreach (var d in input)
            {
                if (d.Quantity <= 0)
                {
                    throw new ValidationException("Line quantity must be positive.");
                }
                if (d.UnitPrice < 0m)
                {
                    throw new ValidationException("Line unit price must be non-negative.");
                }
                if (byProduct.TryGetValue(d.ProductId, out var existing))
                {
                    existing.Quantity += d.Quantity;
                    existing.UnitPrice = d.UnitPrice; // latest price wins
                }
                else
                {
                    byProduct[d.ProductId] = new SaleDetail
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice
                    };
                    order.Add(d.ProductId);
                }
            }
            return order.Select(pid => byProduct[pid]).ToList();
        }

        /// <summary>
        /// Rounds each line's monetary fields to 2 decimal places using banker's
        /// half-even (MidpointRounding.ToEven) so repeated rounding of 0.005 does
        /// not skew the totals. Returns the same input list (in-place rounding).
        /// </summary>
        internal static IList<SaleDetail> RoundLines(IList<SaleDetail> input)
        {
            foreach (var d in input)
            {
                d.UnitPrice = Math.Round(d.UnitPrice, 2, MidpointRounding.ToEven);
            }
            return input;
        }
    }
}
