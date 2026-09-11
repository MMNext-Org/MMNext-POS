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
        private readonly ISalesReturnRepository _salesReturnRepo;
        private readonly ISalesReturnDetailRepository _salesReturnDetailRepo;
        private readonly IReturnNumberGenerator _returnNumberGenerator;
        private readonly IPaymentService _paymentService;

        public SalesService(
            ISaleRepository saleRepo,
            ISaleDetailRepository saleDetailRepo,
            IProductRepository productRepo,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IStockMovementService stockMovementService,
            IInvoiceNumberGenerator invoiceNumberGenerator,
            IInvoiceService invoiceService,
            IOutstandingService outstandingService,
            ISalesReturnRepository salesReturnRepo,
            ISalesReturnDetailRepository salesReturnDetailRepo,
            IReturnNumberGenerator returnNumberGenerator,
            IPaymentService paymentService)
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
            _salesReturnRepo = salesReturnRepo ?? throw new ArgumentNullException(nameof(salesReturnRepo));
            _salesReturnDetailRepo = salesReturnDetailRepo ?? throw new ArgumentNullException(nameof(salesReturnDetailRepo));
            _returnNumberGenerator = returnNumberGenerator ?? throw new ArgumentNullException(nameof(returnNumberGenerator));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            }

        /// <inheritdoc />
        public Task AggregateDuplicateLines(IEnumerable<SaleDetail> details)
        {
            var list = details as IList<SaleDetail> ?? details.ToList();
            var result = AggregateDuplicateLines(list);
            // Return task - this is a helper, the actual aggregation happens in CreateSaleAsync
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task RoundLines(IEnumerable<SaleDetail> details)
        {
            var list = details as IList<SaleDetail> ?? details.ToList();
            RoundLines(list);
return Task.CompletedTask;
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
                sale.TotalAmount = rounded.Sum(l => l.LineTotal);
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

                // 9) Create payment record for the sale amount (cash payment on completion).
                //    This links the payment to the sale so the cashier can track
                //    what was paid vs. what is still outstanding.
                if (createdSale.TotalAmount > 0m && createdSale.CustomerId > 0)
                {
                    var payment = new Payment
                    {
                        PaymentNo = $"PAY-{invoiceNo}",
                        PaymentType = "Customer",
                        SaleId = createdSale.Id,
                        CustomerId = createdSale.CustomerId,
                        Amount = createdSale.TotalAmount,
                        Method = "Cash",
                        PaymentDate = createdSale.SaleDate,
                        Status = "Completed",
                        Description = $"Payment for sale {invoiceNo}"
                    };
                    await _paymentService.AddAsync(payment, cancellationToken).ConfigureAwait(false);
                }

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
        /// Combines repeated lines for the same product into one line. The
        /// last-seen UnitPrice wins, and quantities, discounts, and taxes are summed.
        /// Throws if any line has a non-positive quantity.
        /// </summary>
        public static IList<SaleDetail> AggregateDuplicateLines(IList<SaleDetail> input)
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
                if (d.DiscountAmount < 0m)
                {
                    throw new ValidationException("Line discount amount must be non-negative.");
                }
                if (d.TaxAmount < 0m)
                {
                    throw new ValidationException("Line tax amount must be non-negative.");
                }
                if (byProduct.TryGetValue(d.ProductId, out var existing))
                {
                    existing.Quantity += d.Quantity;
                    existing.UnitPrice = d.UnitPrice;
                    existing.DiscountAmount += d.DiscountAmount;
                    existing.TaxAmount += d.TaxAmount;
                }
                else
                {
                    byProduct[d.ProductId] = new SaleDetail
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        DiscountAmount = d.DiscountAmount,
                        TaxAmount = d.TaxAmount
                    };
                    order.Add(d.ProductId);
                }
            }
            return order.Select(pid => byProduct[pid]).ToList();
        }

        /// <summary>
        /// Rounds each line's monetary fields to 2 decimal places using
        /// banker's half-even (MidpointRounding.ToEven). Calculates LineTotal.
        /// Returns the same input list (in-place rounding).
        /// </summary>
        public static IList<SaleDetail> RoundLines(IList<SaleDetail> input)
        {
            foreach (var d in input)
            {
                d.UnitPrice = Math.Round(d.UnitPrice, 2, MidpointRounding.ToEven);
                d.DiscountAmount = Math.Round(d.DiscountAmount, 2, MidpointRounding.ToEven);
                d.TaxAmount = Math.Round(d.TaxAmount, 2, MidpointRounding.ToEven);
                d.LineTotal = d.CalculateLineTotal();
                d.LineTotal = Math.Round(d.LineTotal, 2, MidpointRounding.ToEven);
            }
            return input;
        }

        /// <inheritdoc />
        public async Task<SalesReturn> ProcessReturnAsync(SalesReturnRequest returnRequest, CancellationToken cancellationToken = default)
        {
            if (returnRequest == null) throw new ArgumentNullException(nameof(returnRequest));
            if (returnRequest.Lines == null || returnRequest.Lines.Count == 0)
            {
                throw new ValidationException("A return must have at least one line item.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // 1) Validate the original sale exists and is in a returnable state
                var sale = await _saleRepo.GetByIdAsync(returnRequest.SaleId, cancellationToken).ConfigureAwait(false);
                if (sale == null)
                {
                    throw new ValidationException($"Sale {returnRequest.SaleId} not found.");
                }

                if (sale.Status == "Voided" || sale.Status == "Returned")
                {
                    throw new ValidationException($"Cannot return a {sale.Status?.ToLower() ?? "unknown"} sale.");
                }

                // 2) Get the original sale details for validation
                var saleDetails = await _saleDetailRepo.GetBySaleIdAsync(returnRequest.SaleId, cancellationToken).ConfigureAwait(false);

                // 3) Validate each return line against the original sale
                foreach (var returnLine in returnRequest.Lines)
                {
                    var originalDetail = saleDetails.FirstOrDefault(d => d.Id == returnLine.SaleDetailId);
                    if (originalDetail == null)
                    {
                        throw new ValidationException($"Sale detail {returnLine.SaleDetailId} not found in the original sale.");
                    }

                    // Check if quantity is valid
                    if (returnLine.Quantity <= 0)
                    {
                        throw new ValidationException($"Return quantity must be positive for product {returnLine.ProductId}.");
                    }

                    // Check if we're not returning more than was sold (accounting for previous returns)
                    var alreadyReturned = await _salesReturnDetailRepo.GetBySaleDetailIdAsync(returnLine.SaleDetailId, cancellationToken);
                    var totalReturnedQty = alreadyReturned.Sum(r => r.Quantity);
                    var availableToReturn = originalDetail.Quantity - totalReturnedQty;

                    if (returnLine.Quantity > availableToReturn)
                    {
                        throw new ValidationException($"Cannot return {returnLine.Quantity} units. Only {availableToReturn} units available for return.");
                    }
                }

                // 4) Generate return number
                var returnNo = await _returnNumberGenerator.NextAsync("RET", CancellationToken.None).ConfigureAwait(false);

                // 5) Create the sales return header
                var salesReturn = new SalesReturn
                {
                    ReturnNo = returnRequest.ReturnNo ?? string.Empty,
                    SaleId = returnRequest.SaleId,
                    CustomerId = returnRequest.CustomerId,
                    ReturnDate = DateTime.UtcNow,
                    TotalAmount = 0m, // Will be calculated
                    Reason = returnRequest.Reason ?? "Customer return",
                    Status = "Active",
                    CreatedByUserId = returnRequest.CreatedByUserId
                };

                var createdReturn = await _salesReturnRepo.AddAsync(salesReturn, cancellationToken).ConfigureAwait(false);

                // 6) Process each return line
                decimal totalReturnAmount = 0m;

                foreach (var returnLine in returnRequest.Lines)
                {
                    var originalDetail = saleDetails.First(d => d.Id == returnLine.SaleDetailId);

                    // Calculate line total
                    var lineTotal = returnLine.Quantity * returnLine.UnitPrice;

                    // Create return detail
                    var returnDetail = new SalesReturnDetail
                    {
                        SalesReturnId = createdReturn.Id,
                        SaleDetailId = returnLine.SaleDetailId,
                        ProductId = returnLine.ProductId,
                        Quantity = returnLine.Quantity,
                        UnitPrice = returnLine.UnitPrice,
                        Reason = returnLine.Reason
                    };

                    await _salesReturnDetailRepo.AddAsync(returnDetail, cancellationToken).ConfigureAwait(false);

                    totalReturnAmount += lineTotal;

                    // 7) Restore stock for the returned product
                    var incrementSuccess = await _productRepo.TryIncrementStockAsync(
                        returnLine.ProductId,
                        returnLine.Quantity,
                        returnRequest.CreatedByUserId ?? 0,
                        returnLine.Reason ?? returnRequest.Reason ?? "Customer return",
                        cancellationToken).ConfigureAwait(false);
                    
                    if (!incrementSuccess)
                    {
                        throw new ValidationException($"Failed to restore stock for product {returnLine.ProductId}. Product may not exist.");
                    }

                    // 8) Create stock movement for the return
                    await _stockMovementService.AddReturnMovementAsync(
                        returnId: createdReturn.Id,
                        productId: returnLine.ProductId,
                        quantity: returnLine.Quantity,
                        unitCost: returnLine.UnitPrice,
                        locationId: null,
                        createdByUserId: returnRequest.CreatedByUserId,
                        reason: returnLine.Reason ?? returnRequest.Reason ?? "Customer return",
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }

                // 7) Update the return header with total amount
                salesReturn.TotalAmount = totalReturnAmount;
                await _salesReturnRepo.UpdateAsync(salesReturn, cancellationToken).ConfigureAwait(false);

                // 8) Update customer outstanding (credit the customer)
                if (returnRequest.CustomerId > 0 && totalReturnAmount > 0m)
                {
                    var credit = new CustomerOutstanding
                    {
                        CustomerId = returnRequest.CustomerId,
                        SaleId = returnRequest.SaleId,
                        TransactionDate = DateTime.UtcNow,
                        DebitAmount = 0m,
                        CreditAmount = totalReturnAmount,
                        Balance = -totalReturnAmount, // Credit reduces balance
                        Description = $"Return {salesReturn.ReturnNo} for sale {returnRequest.SaleId}",
                        Status = "Open"
                    };
                    await _outstandingService.AddCustomerOutstandingAsync(credit, cancellationToken).ConfigureAwait(false);
                }

                // 9) Audit log
                await _auditService.LogAsync(
                    entityName: nameof(SalesReturn),
                    entityId: createdReturn.Id,
                    action: "Create",
                    oldValues: null,
                    newValues: new
                    {
                        ReturnId = createdReturn.Id,
                        ReturnNo = createdReturn.ReturnNo,
                        SaleId = createdReturn.SaleId,
                        CustomerId = createdReturn.CustomerId,
                        TotalAmount = createdReturn.TotalAmount,
                        LineCount = returnRequest.Lines.Count
                    },
                    userId: null,
                    userName: null,
                    description: $"Return {createdReturn.ReturnNo} processed: {returnRequest.Lines.Count} lines, total {totalReturnAmount:C2}",
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return createdReturn;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Sale> VoidSaleAsync(int saleId, string reason, CancellationToken cancellationToken = default)
        {
            if (saleId <= 0)
                throw new ArgumentOutOfRangeException(nameof(saleId), saleId, "Sale ID must be positive.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Void reason is required.", nameof(reason));

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // 1) Get the sale to void
                var sale = await _saleRepo.GetByIdAsync(saleId, cancellationToken).ConfigureAwait(false);
                if (sale == null)
                {
                    throw new ValidationException($"Sale {saleId} not found.");
                }

                // 2) Check if sale can be voided
                if (sale.Status == "Voided")
                {
                    throw new ValidationException($"Sale {saleId} is already voided.");
                }

                if (sale.Status == "Returned" || sale.Status == "PartiallyReturned")
                {
                    throw new ValidationException($"Cannot void a {sale.Status?.ToLower() ?? "returned"} sale. Process a return reversal instead.");
                }

                // 3) Get sale details for stock restoration
                var saleDetails = await _saleDetailRepo.GetBySaleIdAsync(saleId, cancellationToken).ConfigureAwait(false);

                // 4) Restore stock for each line item
                foreach (var detail in saleDetails)
                {
                    var incrementSuccess = await _productRepo.TryIncrementStockAsync(
                        detail.ProductId,
                        detail.Quantity,
                        0, // System user for void
                        $"Void sale #{saleId}: {reason}",
                        cancellationToken).ConfigureAwait(false);

                    if (!incrementSuccess)
                    {
                        throw new ValidationException($"Failed to restore stock for product {detail.ProductId}. Product may not exist.");
                    }

                    // Create stock movement for the void
                    await _stockMovementService.AddVoidMovementAsync(
                        saleId: saleId,
                        productId: detail.ProductId,
                        quantity: detail.Quantity,
                        unitCost: detail.UnitPrice,
                        locationId: sale.LocationId,
                        createdByUserId: 0, // System user
                        reason: $"Void sale #{saleId}: {reason}",
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }

                // 5) Update sale status to Voided
                sale.Status = "Voided";
                await _saleRepo.UpdateAsync(sale, cancellationToken).ConfigureAwait(false);

                // 5) Reverse customer outstanding (credit the customer)
                if (sale.CustomerId > 0 && sale.TotalAmount > 0m)
                {
                    var credit = new CustomerOutstanding
                    {
                        CustomerId = sale.CustomerId,
                        SaleId = sale.Id,
                        TransactionDate = DateTime.UtcNow,
                        DebitAmount = 0m,
                        CreditAmount = sale.TotalAmount,
                        Balance = -sale.TotalAmount,
                        Description = $"Void sale #{sale.Id}: {reason}",
                        Status = "Open"
                    };
                    await _outstandingService.AddCustomerOutstandingAsync(credit, cancellationToken).ConfigureAwait(false);
                }

                // 6) Void the associated invoice if exists
                var invoices = await _invoiceService.GetBySaleIdAsync(saleId, cancellationToken);
                foreach (var invoice in invoices)
                {
                    if (invoice.Status == "Active")
                    {
                        invoice.Status = "Voided";
                        await _invoiceService.UpdateAsync(invoice, cancellationToken).ConfigureAwait(false);
                    }
                }

                // 7) Create stock movement for the void (header level)
                await _stockMovementService.AddVoidMovementAsync(
                    saleId: saleId,
                    productId: 0, // Header level
                    quantity: 0,
                    unitCost: 0,
                    locationId: sale.LocationId,
                    createdByUserId: 0,
                    reason: $"Void sale #{saleId}: {reason}",
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                // 6) Audit log
                await _auditService.LogAsync(
                    entityName: nameof(Sale),
                    entityId: sale.Id,
                    action: "Void",
                    oldValues: new { Status = "Completed" },
                    newValues: new { Status = "Voided" },
                    userId: null,
                    userName: null,
                    description: $"Sale #{sale.Id} voided: {reason}",
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
                return sale;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
        }
    }
}
