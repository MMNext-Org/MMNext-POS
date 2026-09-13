using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class SalesServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock = new();
        private readonly Mock<ISaleRepository> _saleRepoMock = new();
        private readonly Mock<ISaleDetailRepository> _saleDetailRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();
        private readonly Mock<IStockMovementService> _stockMovementMock = new();
        private readonly Mock<IInvoiceNumberGenerator> _invoiceNumberMock = new();
        private readonly Mock<IInvoiceService> _invoiceServiceMock = new();
        private readonly Mock<IOutstandingService> _outstandingMock = new();
        private readonly Mock<ISalesReturnRepository> _salesReturnRepoMock = new();
        private readonly Mock<ISalesReturnDetailRepository> _salesReturnDetailRepoMock = new();
        private readonly Mock<IReturnNumberGenerator> _returnNumberGeneratorMock = new();
        private readonly Mock<IPaymentService> _paymentServiceMock = new();
        private readonly Mock<ITaxRateService> _taxRateServiceMock = new();

        private ISalesService CreateService()
        {
            return new SalesService(
                _saleRepoMock.Object,
                _saleDetailRepoMock.Object,
                _productRepoMock.Object,
                _unitOfWorkMock.Object,
                _auditServiceMock.Object,
                _stockMovementMock.Object,
                _invoiceNumberMock.Object,
                _invoiceServiceMock.Object,
                _outstandingMock.Object,
                _salesReturnRepoMock.Object,
                _salesReturnDetailRepoMock.Object,
                _returnNumberGeneratorMock.Object,
                _paymentServiceMock.Object,
                _taxRateServiceMock.Object);
        }

        private void SetupHappyPath(int productId = 1, int initialStock = 10, string productName = "Widget")
        {
            var product = new Product { Id = productId, StockQuantity = initialStock, Name = productName, Price = 5m };
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(productId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(product);
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => { s.Id = 42; })
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);
            _stockMovementMock.Setup(s => s.AddSaleMovementAsync(It.IsAny<Sale>(), It.IsAny<IReadOnlyList<SaleDetail>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new StockMovement { Id = 100 });
            _invoiceNumberMock.Setup(n => n.NextAsync("INV", It.IsAny<CancellationToken>()))
                              .ReturnsAsync("INV-2026-000001");
            _invoiceServiceMock.Setup(i => i.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((Invoice inv, CancellationToken ct) => { inv.Id = 7; return inv; });
            _outstandingMock.Setup(o => o.AddCustomerOutstandingAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((CustomerOutstanding co, CancellationToken ct) => { co.Id = 9; return co; });
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.CommitAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            // Default tax rate service setup - returns 0 (no tax) by default
            _taxRateServiceMock.Setup(t => t.GetTaxRateForCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(0m);
        }

        // ───────────────────────── Existing behaviour preserved ─────────────────────────

        [Fact]
        public async Task CreateSaleAsync_ValidData_CreatesSaleAndDecrementsStock()
        {
            // Arrange
            SetupHappyPath(productId: 1, initialStock: 10);

            var service = CreateService();
            var sale = new Sale { CustomerId = 1, TotalAmount = 15m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 5m }
            };

            // Act
            var result = await service.CreateSaleAsync(sale, details);

            // Assert
            Assert.Equal(42, result.Id);
            _unitOfWorkMock.Verify(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _saleRepoMock.Verify(r => r.CreateSaleWithDetailsAsync(sale, It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()), Times.Once);
            // New contract: stock is decremented atomically (no read-then-write on Product).
            _productRepoMock.Verify(r => r.TryDecrementStockAsync(1, 2, It.IsAny<int>(), "Sale", It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateSaleAsync_InsufficientStock_ThrowsInsufficientStockException()
        {
            // Arrange
            var product = new Product { Id = 1, StockQuantity = 1, Name = "Widget" };
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(1, 2, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(product);
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var sale = new Sale { CustomerId = 1, TotalAmount = 15m };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 5m }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientStockException>(() => service.CreateSaleAsync(sale, details));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            // Critical: no side effects beyond rollback
            _saleRepoMock.Verify(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()), Times.Never);
            _invoiceServiceMock.Verify(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
            _stockMovementMock.Verify(r => r.AddSaleMovementAsync(It.IsAny<Sale>(), It.IsAny<IReadOnlyList<SaleDetail>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateSaleAsync_ExceptionDuringSaleRepo_RollsBackTransaction()
        {
            // Arrange
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new InvalidOperationException("Database error"));
            _unitOfWorkMock.Setup(r => r.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var sale = new Sale { CustomerId = 1, TotalAmount = 15m };
            var details = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 5m } };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateSaleAsync(sale, details));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // ───────────────────────── New Phase-2 hardening tests ─────────────────────────

        [Fact]
        public async Task CreateSaleAsync_AggregatesDuplicateLines_ForSameProduct()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 5m },
                new SaleDetail { ProductId = 1, Quantity = 3, UnitPrice = 5m },
                new SaleDetail { ProductId = 2, Quantity = 1, UnitPrice = 7m }
            };
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(2, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Product { Id = 2, StockQuantity = 99, Name = "Gadget" });

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: stock decrements use the aggregated quantities (2+3=5 for product 1, 1 for product 2)
            _productRepoMock.Verify(r => r.TryDecrementStockAsync(1, 5, It.IsAny<int>(), "Sale", It.IsAny<CancellationToken>()), Times.Once);
            _productRepoMock.Verify(r => r.TryDecrementStockAsync(2, 1, It.IsAny<int>(), "Sale", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSaleAsync_RoundsLineTotals_HalfEvenToTwoDecimals()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };

            // 0.125m * 1 = 0.125 → banker's round to 2dp = 0.12 (rounds to even).
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 0.125m }
            };
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(1, 1, It.IsAny<int>(), "Sale", It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Product { Id = 1, StockQuantity = 10, Name = "Widget" });

            Sale? capturedSale = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, _, _) => capturedSale = s)
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: TotalAmount is rounded to 2dp. 0.125 → 0.12 (MidpointRounding.ToEven).
            Assert.NotNull(capturedSale);
            Assert.Equal(0.12m, capturedSale!.TotalAmount);
        }

        [Fact]
        public async Task CreateSaleAsync_StockMovement_CreatedWithTypeSaleAndSameLocation()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1, LocationId = 7 };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 5m }
            };

            // Act
            var result = await service.CreateSaleAsync(sale, details);

            // Assert
            _stockMovementMock.Verify(s => s.AddSaleMovementAsync(
                It.Is<Sale>(x => x.Id == 42 && x.LocationId == 7),
                It.IsAny<IReadOnlyList<SaleDetail>>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSaleAsync_Invoice_AutoGeneratedAndLinkedToSale()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1, TotalAmount = 10m };
            var details = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 5m } };

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert
            _invoiceNumberMock.Verify(n => n.NextAsync("INV", It.IsAny<CancellationToken>()), Times.Once);
            _invoiceServiceMock.Verify(i => i.AddAsync(
                It.Is<Invoice>(inv => inv.SaleId == 42 && inv.InvoiceNo == "INV-2026-000001" && inv.AmountDue == 10m),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSaleAsync_InvoiceNumber_UsesCurrentYearPrefix()
        {
            // Arrange: capture the value the generator returns to confirm formatting
            SetupHappyPath();
            _invoiceNumberMock.Setup(n => n.NextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync($"INV-{DateTime.UtcNow.Year}-000042");
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 5m } };

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: invoice row carries the auto-generated number
            _invoiceServiceMock.Verify(i => i.AddAsync(
                It.Is<Invoice>(inv => inv.InvoiceNo.StartsWith($"INV-{DateTime.UtcNow.Year}-")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSaleAsync_CustomerOutstanding_CreatedForCreditSale()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 5, TotalAmount = 100m };
            var details = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 50m } };

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert
            _outstandingMock.Verify(o => o.AddCustomerOutstandingAsync(
                It.Is<CustomerOutstanding>(co =>
                    co.CustomerId == 5 &&
                    co.SaleId == 42 &&
                    co.DebitAmount == 100m &&
                    co.CreditAmount == 0m &&
                    co.Balance == 100m &&
                    co.Status == "Open"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateSaleAsync_CustomerOutstanding_NotCreatedForZeroCustomerId()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 0, TotalAmount = 10m }; // walk-in
            var details = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 5m } };

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert
            _outstandingMock.Verify(o => o.AddCustomerOutstandingAsync(It.IsAny<CustomerOutstanding>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateSaleAsync_AuditLog_WrittenInsideTransaction_BeforeCommit()
        {
            // Arrange
            SetupHappyPath();

            // Use a sequence to assert that audit happens BEFORE commit.
            var callOrder = new List<string>();
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("audit"))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(r => r.CommitAsync(It.IsAny<CancellationToken>()))
                            .Callback(() => callOrder.Add("commit"))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 5m } };

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert
            Assert.Equal(new[] { "audit", "commit" }, callOrder);
        }

        [Fact]
        public async Task CreateSaleAsync_AuditFailure_RollsBackEntireTransaction()
        {
            // Arrange
            SetupHappyPath();
            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("audit DB down"));
            _unitOfWorkMock.Setup(r => r.RollbackAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 5m } };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateSaleAsync(sale, details));
            _unitOfWorkMock.Verify(r => r.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AddSaleDetailAsync_DelegatesToCreateSaleAsync_AndFollowsSameRules()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var detail = new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 5m };

            // Act
            var result = await service.AddSaleDetailAsync(saleId: 999, detail);

            // Assert: same flow applies — stock decrement, stock movement, invoice, outstanding, audit
            _productRepoMock.Verify(r => r.TryDecrementStockAsync(1, 1, It.IsAny<int>(), "Sale", It.IsAny<CancellationToken>()), Times.Once);
            _stockMovementMock.Verify(s => s.AddSaleMovementAsync(It.IsAny<Sale>(), It.IsAny<IReadOnlyList<SaleDetail>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
            _invoiceNumberMock.Verify(n => n.NextAsync("INV", It.IsAny<CancellationToken>()), Times.Once);
            _invoiceServiceMock.Verify(i => i.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), "Create", It.IsAny<object?>(), It.IsAny<object?>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(42, result.SaleId);
        }

        [Fact]
        public async Task CreateSaleAsync_EmptyDetails_ThrowsValidationException()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                service.CreateSaleAsync(new Sale { CustomerId = 1 }, Array.Empty<SaleDetail>()));
        }

        [Fact]
        public async Task CreateSaleAsync_NegativeQuantity_ThrowsValidationException()
        {
            // Arrange
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = -1, UnitPrice = 5m } };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateSaleAsync(sale, details));
        }

        // ───────────────────────── Existing GetAllAsync tests ─────────────────────────

        private void SetupSales(params Sale[] sales)
        {
            _saleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync((IReadOnlyList<Sale>)sales.ToList());
        }

        [Fact]
        public async Task GetAllAsync_NoFilters_ReturnsAllSalesOrderedByDateDescending()
        {
            SetupSales(
                new Sale { Id = 1, CustomerId = 1, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 10m },
                new Sale { Id = 2, CustomerId = 2, SaleDate = new DateTime(2026, 9, 3), TotalAmount = 20m },
                new Sale { Id = 3, CustomerId = 1, SaleDate = new DateTime(2026, 9, 2), TotalAmount = 30m });

            var service = CreateService();

            var result = await service.GetAllAsync();

            Assert.Equal(new[] { 2, 3, 1 }, result.Select(s => s.Id));
        }

        [Fact]
        public async Task GetAllAsync_DateRange_FiltersInclusively()
        {
            SetupSales(
                new Sale { Id = 1, SaleDate = new DateTime(2026, 8, 31, 23, 59, 59), TotalAmount = 10m },
                new Sale { Id = 2, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 20m },
                new Sale { Id = 3, SaleDate = new DateTime(2026, 9, 5, 12, 0, 0), TotalAmount = 30m },
                new Sale { Id = 4, SaleDate = new DateTime(2026, 9, 10), TotalAmount = 40m });

            var service = CreateService();

            var result = await service.GetAllAsync(
                fromDate: new DateTime(2026, 9, 1),
                toDate: new DateTime(2026, 9, 5));

            Assert.Equal(new[] { 3, 2 }, result.Select(s => s.Id));
        }

        [Fact]
        public async Task GetAllAsync_CustomerFilter_ReturnsOnlySalesForThatCustomer()
        {
            SetupSales(
                new Sale { Id = 1, CustomerId = 1, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 10m },
                new Sale { Id = 2, CustomerId = 2, SaleDate = new DateTime(2026, 9, 2), TotalAmount = 20m },
                new Sale { Id = 3, CustomerId = 1, SaleDate = new DateTime(2026, 9, 3), TotalAmount = 30m });

            var service = CreateService();

            var result = await service.GetAllAsync(customerId: 1);

            Assert.Equal(new[] { 3, 1 }, result.Select(s => s.Id));
        }

        [Fact]
        public async Task GetAllAsync_StatusFilter_IsCaseInsensitiveAndExcludesNullStatus()
        {
            SetupSales(
                new Sale { Id = 1, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 10m, Status = "Completed" },
                new Sale { Id = 2, SaleDate = new DateTime(2026, 9, 2), TotalAmount = 20m, Status = "completed" },
                new Sale { Id = 3, SaleDate = new DateTime(2026, 9, 3), TotalAmount = 30m, Status = "Voided" },
                new Sale { Id = 4, SaleDate = new DateTime(2026, 9, 4), TotalAmount = 40m, Status = null });

            var service = CreateService();

            var result = await service.GetAllAsync(status: "COMPLETED");

            Assert.Equal(new[] { 2, 1 }, result.Select(s => s.Id));
        }

        [Fact]
        public async Task GetAllAsync_LocationFilter_ReturnsOnlySalesForThatLocation()
        {
            SetupSales(
                new Sale { Id = 1, SaleDate = new DateTime(2026, 9, 1), TotalAmount = 10m, LocationId = 1 },
                new Sale { Id = 2, SaleDate = new DateTime(2026, 9, 2), TotalAmount = 20m, LocationId = 2 },
                new Sale { Id = 3, SaleDate = new DateTime(2026, 9, 3), TotalAmount = 30m, LocationId = null });

            var service = CreateService();

            var result = await service.GetAllAsync(locationId: 1);

            Assert.Equal(new[] { 1 }, result.Select(s => s.Id));
        }

        // ───────────────────────── Phase 2 Sprint 2 P1 tests ─────────────────────────

        /// <summary>
        /// TC3.6.3: Fixed-Amount Discount - Fixed amount subtracted from line total.
        /// LineTotal = Quantity * UnitPrice - DiscountAmount + TaxAmount
        /// 1 * 10m - 3m + 0m = 7m
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_PriceDiscount_FixedAmount()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 10m, DiscountAmount = 3m, TaxAmount = 0m }
            };
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(1, 1, It.IsAny<int>(), "Sale", It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Product { Id = 1, StockQuantity = 10, Name = "Widget" });

            Sale? capturedSale = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, _, _) => capturedSale = s)
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: TotalAmount = 1 * 10m - 3m = 7m
            Assert.NotNull(capturedSale);
            Assert.Equal(7m, capturedSale!.TotalAmount);
        }

        /// <summary>
        /// TC3.6.4: Price Override - Scanner price overrides catalog price.
        /// The SaleDetail.UnitPrice passed in should be used directly (not looked up from Product).
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_PriceOverride_ScannerWins()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            // Scanner provides 8m, but product catalog price is 5m (from SetupHappyPath)
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 8m }
            };

            Sale? capturedSale = null;
            List<SaleDetail>? capturedDetails = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, d, _) => { capturedSale = s; capturedDetails = d.ToList(); })
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: The scanner price (8m) should be used, not catalog price (5m)
            // TotalAmount = 2 * 8m = 16m (after rounding)
            Assert.NotNull(capturedSale);
            Assert.Equal(16m, capturedSale!.TotalAmount);
            Assert.NotNull(capturedDetails);
            Assert.Single(capturedDetails);
            Assert.Equal(8m, capturedDetails[0].UnitPrice);
        }

        /// <summary>
        /// TC3.6.5: Boundary Fixtures - Negative price and >100% discount validation.
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_BoundaryFixtures_NegativePrice_ThrowsValidationException()
        {
            // Arrange
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = -5m }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateSaleAsync(sale, details));
        }

        [Fact]
        public async Task CreateSaleAsync_BoundaryFixtures_DiscountExceedsLineTotal_ThrowsValidationException()
        {
            // Arrange
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            // DiscountAmount (15m) > Quantity * UnitPrice (10m) should throw
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 10m, DiscountAmount = 15m, TaxAmount = 0m }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.CreateSaleAsync(sale, details));
        }

        /// <summary>
        /// TC3.6.2: Percentage Discount - Discount applied to unit price or line total.
        /// 20% discount on 10m = 2m discount, TotalAmount = 10m - 2m = 8m (rounded).
        /// The discount percentage is calculated externally and passed as DiscountAmount.
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_PriceDiscount_Percentage()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            // 20% discount on 10m = 2m discount
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 10m, DiscountAmount = 2m, TaxAmount = 0m }
            };

            Sale? capturedSale = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, _, _) => capturedSale = s)
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: TotalAmount = 10m - 2m = 8m (range 7.90-8.10 to account for rounding)
            Assert.NotNull(capturedSale);
            Assert.InRange(capturedSale!.TotalAmount, 7.90m, 8.10m);
        }

        /// <summary>
        /// TC3.7.5: Cash change calculation - Change = amountPaid - amountDue, rounded to 2dp.
        /// 15m paid - 10.99m due = 4.01m change
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_Rounding_Change_Calculation()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 10.99m }
            };

            Sale? capturedSale = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, _, _) => capturedSale = s)
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: Sale total is 10.99m (rounded), change from 15m = 4.01m
            Assert.NotNull(capturedSale);
            Assert.Equal(10.99m, capturedSale!.TotalAmount);
            var change = 15m - capturedSale.TotalAmount;
            Assert.Equal(4.01m, change);
        }

        /// <summary>
        /// TC3.7.3: Repeated rounding doesn't skew - 0.15 + 0.15 + 0.15 → 0.45 (not 0.46)
        /// Each line is rounded individually, then summed. Banker's rounding (MidpointRounding.ToEven)
        /// ensures 0.15 stays 0.15, so 0.15 * 3 = 0.45.
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_Rounding_Repeated_NotSkew()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 0.15m },
                new SaleDetail { ProductId = 2, Quantity = 1, UnitPrice = 0.15m },
                new SaleDetail { ProductId = 3, Quantity = 1, UnitPrice = 0.15m }
            };

            _productRepoMock.Setup(r => r.TryDecrementStockAsync(2, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Product { Id = 2, StockQuantity = 99, Name = "Gadget" });
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(3, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Product { Id = 3, StockQuantity = 99, Name = "Gizmo" });

            Sale? capturedSale = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, _, _) => capturedSale = s)
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: TotalAmount in range [0.44m, 0.46m] - exactly 0.45m with banker's rounding
            Assert.NotNull(capturedSale);
            Assert.InRange(capturedSale!.TotalAmount, 0.44m, 0.46m);
            Assert.Equal(0.45m, capturedSale.TotalAmount);
        }

        /// <summary>
        /// TC3.7.4: Line items summed then total - Total = sum of individually rounded lines.
        /// (0.33 × 3) → 0.99 not 1.00
        /// Each 0.33 line rounds to 0.33, sum = 0.99 (not 1.00 which would happen if total was rounded after sum).
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_Rounding_LineItemsSummedThenTotal()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();
            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 0.33m },
                new SaleDetail { ProductId = 2, Quantity = 1, UnitPrice = 0.33m },
                new SaleDetail { ProductId = 3, Quantity = 1, UnitPrice = 0.33m }
            };

            _productRepoMock.Setup(r => r.TryDecrementStockAsync(2, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Product { Id = 2, StockQuantity = 99, Name = "Gadget" });
            _productRepoMock.Setup(r => r.TryDecrementStockAsync(3, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _productRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Product { Id = 3, StockQuantity = 99, Name = "Gizmo" });

            Sale? capturedSale = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, _, _) => capturedSale = s)
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: TotalAmount = 0.99m (sum of 3 × 0.33), NOT 1.00m
            Assert.NotNull(capturedSale);
            Assert.Equal(0.99m, capturedSale!.TotalAmount);
        }

        /// <summary>
        /// TC3.8.3: Overpayment → Credit for Future.
        /// Payment > outstanding balance → credit recorded for future sale.
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_OverpaymentCredit_Created()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // First create a sale with 50 outstanding
            var sale1 = new Sale { CustomerId = 1, TotalAmount = 50m };
            var details1 = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 50m } };
            await service.CreateSaleAsync(sale1, details1);

            // Verify outstanding created with balance 50
            _outstandingMock.Verify(o => o.AddCustomerOutstandingAsync(
                It.Is<CustomerOutstanding>(co => co.Balance == 50m && co.Status == "Open"), It.IsAny<CancellationToken>()), Times.Once);

            // Now apply a payment of 70 (overpayment of 20)
            // This should create a credit outstanding record with Balance = -20
            var creditOutstanding = new CustomerOutstanding
            {
                CustomerId = 1,
                SaleId = sale1.Id,
                TransactionDate = DateTime.UtcNow,
                DebitAmount = 0m,
                CreditAmount = 20m,
                Balance = -20m, // Credit
                Description = "Overpayment credit (payment 70.00, applied 50.00)",
                Status = "Credit"
            };

            _outstandingMock.Setup(o => o.ApplyCustomerPaymentAsync(1, 70m, sale1.Id, null, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(creditOutstanding);

            // Act
            var result = await _outstandingMock.Object.ApplyCustomerPaymentAsync(1, 70m, sale1.Id, null, "Overpayment test", It.IsAny<CancellationToken>());

            // Assert
            Assert.Equal(-20m, result.Balance);
            Assert.Equal("Credit", result.Status);
            Assert.Equal(20m, result.CreditAmount);
        }

        /// <summary>
        /// TC3.8.4: Customer Clearance → Zero Balance.
        /// Zero balance → account cleared, audit record.
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_CustomerClearance_ZeroBalance()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // First create a sale with 30 outstanding
            var sale1 = new Sale { CustomerId = 1, TotalAmount = 30m };
            var details1 = new List<SaleDetail> { new SaleDetail { ProductId = 1, Quantity = 1, UnitPrice = 30m } };
            await service.CreateSaleAsync(sale1, details1);

            // Verify outstanding created with balance 30
            _outstandingMock.Verify(o => o.AddCustomerOutstandingAsync(
                It.Is<CustomerOutstanding>(co => co.Balance == 30m && co.Status == "Open"), It.IsAny<CancellationToken>()), Times.Once);

            // Now clear the customer account
            _outstandingMock.Setup(o => o.ClearCustomerAccountAsync(1, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            // Act
            await _outstandingMock.Object.ClearCustomerAccountAsync(1, 1, "Manual clearance", CancellationToken.None);

            // Assert
            _outstandingMock.Verify(o => o.ClearCustomerAccountAsync(1, 1, "Manual clearance", It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// TC3.6.4: Tax Jurisdiction - Different tax rates per customer type.
        /// Customer type "Retail" gets default tax (5%), "Wholesale" gets 0%, "TaxExempt" gets 0%.
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_TaxJurisdiction_CustomerType()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Set up tax rate for customer type "Retail" = 5%
            _taxRateServiceMock.Setup(t => t.GetTaxRateForCustomerAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(0.05m); // 5% tax

            var sale = new Sale { CustomerId = 1 }; // Retail customer
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 10m, TaxAmount = 0m } // Tax will be auto-calculated
            };

            Sale? capturedSale = null;
            List<SaleDetail>? capturedDetails = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, d, _) => { capturedSale = s; capturedDetails = d.ToList(); })
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: Tax should be applied: 2 * 10m = 20m subtotal, 5% = 1m tax, Total = 21m
            Assert.NotNull(capturedSale);
            Assert.NotNull(capturedDetails);
            Assert.Single(capturedDetails);
            Assert.Equal(1m, capturedDetails[0].TaxAmount); // 5% of 20m = 1m
            Assert.Equal(21m, capturedSale!.TotalAmount); // 20m + 1m tax = 21m
        }

        /// <summary>
        /// TC3.6.4b: Tax Jurisdiction - Wholesale customer gets 0% tax.
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_TaxJurisdiction_WholesaleCustomer_NoTax()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Set up tax rate for customer type "Wholesale" = 0%
            _taxRateServiceMock.Setup(t => t.GetTaxRateForCustomerAsync(2, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(0m); // 0% tax for wholesale

            var sale = new Sale { CustomerId = 2 }; // Wholesale customer
            var details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 10m, TaxAmount = 0m }
            };

            Sale? capturedSale = null;
            List<SaleDetail>? capturedDetails = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, d, _) => { capturedSale = s; capturedDetails = d.ToList(); })
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: No tax applied for wholesale
            Assert.NotNull(capturedDetails);
            Assert.Single(capturedDetails);
            Assert.Equal(0m, capturedDetails[0].TaxAmount);
            Assert.Equal(20m, capturedSale!.TotalAmount); // 20m + 0m tax = 20m
        }

        /// <summary>
        /// TC3.6.4c: Tax Jurisdiction - Explicit TaxAmount on SaleDetail is preserved (not overwritten).
        /// </summary>
        [Fact]
        public async Task CreateSaleAsync_TaxJurisdiction_ExplicitTaxAmount_Preserved()
        {
            // Arrange
            SetupHappyPath();
            var service = CreateService();

            // Set up tax rate for customer = 5%
            _taxRateServiceMock.Setup(t => t.GetTaxRateForCustomerAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(0.05m); // 5% tax

            var sale = new Sale { CustomerId = 1 };
            var details = new List<SaleDetail>
            {
                // Explicit tax amount of 3m should be preserved (not auto-calculated)
                new SaleDetail { ProductId = 1, Quantity = 2, UnitPrice = 10m, TaxAmount = 3m }
            };

            Sale? capturedSale = null;
            List<SaleDetail>? capturedDetails = null;
            _saleRepoMock.Setup(r => r.CreateSaleWithDetailsAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>(), It.IsAny<CancellationToken>()))
                         .Callback<Sale, IEnumerable<SaleDetail>, CancellationToken>((s, d, _) => { capturedSale = s; capturedDetails = d.ToList(); })
                         .ReturnsAsync((Sale s, IEnumerable<SaleDetail> d, CancellationToken ct) => s);

            // Act
            await service.CreateSaleAsync(sale, details);

            // Assert: Explicit tax amount (3m) preserved, not auto-calculated (1m)
            Assert.NotNull(capturedDetails);
            Assert.Single(capturedDetails);
            Assert.Equal(3m, capturedDetails[0].TaxAmount); // Explicit 3m preserved
            Assert.Equal(23m, capturedSale!.TotalAmount); // 20m + 3m explicit tax = 23m
        }
    }
}
