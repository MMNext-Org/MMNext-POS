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

        // ───────────────────────── Phase 4: New Movement Types ─────────────────────────

        public async Task<StockMovement> AddIssueMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string reasonCode,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for an issue movement.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");
            if (string.IsNullOrWhiteSpace(reasonCode))
                throw new ArgumentException("Reason code is required for issue movements.", nameof(reasonCode));

            var movementNo = $"SM-ISS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.Issue.ToDbString(),
                ReasonCode = reasonCode,
                Reason = reason ?? $"Issue #{productId}",
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                ProductId = productId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
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
                Notes = $"Issue: {reasonCode}"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddReceiveMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string reasonCode,
            string? reason,
            int? createdByUserId,
            int? supplierId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for a receive movement.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");
            if (string.IsNullOrWhiteSpace(reasonCode))
                throw new ArgumentException("Reason code is required for receive movements.", nameof(reasonCode));

            var movementNo = $"SM-RCV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.Receive.ToDbString(),
                ReasonCode = reasonCode,
                Reason = reason ?? $"Receive #{productId}",
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                ProductId = productId,
                SupplierId = supplierId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
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
                Notes = $"Receive: {reasonCode}"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddDamagedMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for a damaged movement.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");

            var movementNo = $"SM-DMG-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.Damaged.ToDbString(),
                ReasonCode = "DAMAGED",
                Reason = reason ?? "Damaged goods write-off",
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                ProductId = productId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
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
                Notes = "Damaged goods write-off"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddLostMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for a lost movement.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");

            var movementNo = $"SM-LST-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.Lost.ToDbString(),
                ReasonCode = "LOST",
                Reason = reason ?? "Lost/missing stock write-off",
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                ProductId = productId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
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
                Notes = "Lost/missing stock write-off"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddAdjustmentMovementAsync(
            int? locationId,
            int productId,
            int quantity, // Positive = increase, negative = decrease
            decimal unitCost,
            string reasonCode,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default)
        {
            if (quantity == 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity cannot be zero for an adjustment movement.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");
            if (string.IsNullOrWhiteSpace(reasonCode))
                throw new ArgumentException("Reason code is required for adjustment movements.", nameof(reasonCode));

            var movementNo = $"SM-ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.Adjust.ToDbString(),
                ReasonCode = reasonCode,
                Reason = reason ?? $"Adjustment #{productId}",
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                ProductId = productId,
                Quantity = Math.Abs(quantity), // Store absolute value; sign inferred from context
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                IsActive = true
            };

            var created = await _movementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            var detailRow = new StockMovementDetail
            {
                StockMovementId = created.Id,
                ProductId = productId,
                Quantity = Math.Abs(quantity),
                UnitCost = unitCost,
                LineTotal = Math.Abs(quantity) * unitCost,
                Notes = $"Adjustment: {reasonCode} ({(quantity > 0 ? "+" : "")}{quantity})"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddTransferOutMovementAsync(
            int fromLocationId,
            int toLocationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int transferId,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for transfer out.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");
            if (fromLocationId == toLocationId)
                throw new ArgumentException("From and to locations must be different.", nameof(toLocationId));

            var movementNo = $"SM-XOUT-{DateTime.UtcNow:yyyyMMddHHmmss}-T-{transferId}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.TransferOut.ToDbString(),
                ReasonCode = "TRANSFER_OUT",
                Reason = reason ?? $"Transfer to location {toLocationId}",
                MovementDate = DateTime.UtcNow,
                LocationId = fromLocationId,
                FromLocationId = fromLocationId,
                ToLocationId = toLocationId,
                ProductId = productId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = transferId,
                ReferenceType = "StockTransfer",
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
                LocationId = fromLocationId,
                Notes = $"Transfer out to location {toLocationId} (Transfer #{transferId})"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddTransferInMovementAsync(
            int fromLocationId,
            int toLocationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int transferId,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for transfer in.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");
            if (fromLocationId == toLocationId)
                throw new ArgumentException("From and to locations must be different.", nameof(toLocationId));

            var movementNo = $"SM-XIN-{DateTime.UtcNow:yyyyMMddHHmmss}-T-{transferId}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.TransferIn.ToDbString(),
                ReasonCode = "TRANSFER_IN",
                Reason = reason ?? $"Transfer from location {fromLocationId}",
                MovementDate = DateTime.UtcNow,
                LocationId = toLocationId,
                FromLocationId = fromLocationId,
                ToLocationId = toLocationId,
                ProductId = productId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = transferId,
                ReferenceType = "StockTransfer",
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
                LocationId = toLocationId,
                Notes = $"Transfer in from location {fromLocationId} (Transfer #{transferId})"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddAssemblyMovementAsync(
            int assemblyId,
            IReadOnlyList<AssemblyDetail> components,
            int? createdByUserId,
            CancellationToken cancellationToken = default)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Assembly must have at least one component.", nameof(components));

            var movementNo = $"SM-ASM-{DateTime.UtcNow:yyyyMMddHHmmss}-A-{assemblyId}";

            var totalQty = components.Sum(c => c.Quantity);

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.Assembly.ToDbString(),
                ReasonCode = "ASSEMBLY",
                Reason = $"Assembly build #{assemblyId}",
                MovementDate = DateTime.UtcNow,
                Quantity = totalQty,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = assemblyId,
                ReferenceType = "Assembly",
                IsActive = true
            };

            var created = await _movementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            foreach (var c in components)
            {
                var detailRow = new StockMovementDetail
                {
                    StockMovementId = created.Id,
                    ProductId = c.ComponentProductId,
                    Quantity = c.Quantity,
                    UnitCost = c.UnitCost,
                    LineTotal = c.Quantity * c.UnitCost,
                    ReferenceDetailId = c.Id,
                    Notes = $"Assembly component for Assembly #{assemblyId}"
                };
                await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);
            }

            return created;
        }

        public async Task<StockMovement> AddDeassemblyMovementAsync(
            int assemblyId,
            IReadOnlyList<AssemblyDetail> components,
            int? createdByUserId,
            CancellationToken cancellationToken = default)
        {
            if (components == null || components.Count == 0)
                throw new ArgumentException("Deassembly must have at least one component.", nameof(components));

            var movementNo = $"SM-DASM-{DateTime.UtcNow:yyyyMMddHHmmss}-A-{assemblyId}";

            var totalQty = components.Sum(c => c.Quantity);

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.Deassembly.ToDbString(),
                ReasonCode = "DEASSEMBLY",
                Reason = $"Deassembly #{assemblyId}",
                MovementDate = DateTime.UtcNow,
                Quantity = totalQty,
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = assemblyId,
                ReferenceType = "Assembly",
                IsActive = true
            };

            var created = await _movementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            foreach (var c in components)
            {
                var detailRow = new StockMovementDetail
                {
                    StockMovementId = created.Id,
                    ProductId = c.ComponentProductId,
                    Quantity = c.Quantity,
                    UnitCost = c.UnitCost,
                    LineTotal = c.Quantity * c.UnitCost,
                    ReferenceDetailId = c.Id,
                    Notes = $"Deassembly component for Assembly #{assemblyId}"
                };
                await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);
            }

            return created;
        }

        public async Task<StockMovement> AddExpiredMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? batchNumber,
            DateTime? expiryDate,
            int? createdByUserId,
            CancellationToken cancellationToken = default)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive for an expired movement.");
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");

            var movementNo = $"SM-EXP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.Expired.ToDbString(),
                ReasonCode = "EXPIRED",
                Reason = $"Expired goods write-off" + (batchNumber != null ? $" (Batch: {batchNumber})" : "") + (expiryDate.HasValue ? $" (Expiry: {expiryDate.Value:yyyy-MM-dd})" : ""),
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                ProductId = productId,
                Quantity = quantity,
                Status = "Active",
                CreatedByUserId = createdByUserId,
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
                BatchNumber = batchNumber,
                ExpiryDate = expiryDate,
                LocationId = locationId,
                Notes = "Expired goods write-off"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }

        public async Task<StockMovement> AddCycleCountMovementAsync(
            int? locationId,
            int productId,
            int countedQuantity,
            int systemQuantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            CancellationToken cancellationToken = default)
        {
            if (unitCost < 0m)
                throw new ArgumentOutOfRangeException(nameof(unitCost), unitCost, "Unit cost must be non-negative.");

            var variance = countedQuantity - systemQuantity;
            if (variance == 0)
                throw new ArgumentException("Cycle count variance is zero; no movement needed.", nameof(countedQuantity));

            var movementNo = $"SM-CC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";

            var movement = new StockMovement
            {
                MovementNo = movementNo,
                MovementType = StockMovementType.CycleCount.ToDbString(),
                ReasonCode = "CYCLE_COUNT",
                Reason = reason ?? $"Cycle count variance: {(variance > 0 ? "+" : "")}{variance} (Counted: {countedQuantity}, System: {systemQuantity})",
                MovementDate = DateTime.UtcNow,
                LocationId = locationId,
                ProductId = productId,
                Quantity = Math.Abs(variance),
                Status = "Active",
                CreatedByUserId = createdByUserId,
                ReferenceId = referenceId,
                ReferenceType = "CycleCount",
                IsActive = true
            };

            var created = await _movementRepo.AddAsync(movement, cancellationToken).ConfigureAwait(false);

            var detailRow = new StockMovementDetail
            {
                StockMovementId = created.Id,
                ProductId = productId,
                Quantity = Math.Abs(variance),
                UnitCost = unitCost,
                LineTotal = Math.Abs(variance) * unitCost,
                LocationId = locationId,
                Notes = $"Cycle count variance: {(variance > 0 ? "+" : "")}{variance} (Counted: {countedQuantity}, System: {systemQuantity})"
            };
            await _detailRepo.AddAsync(detailRow, cancellationToken).ConfigureAwait(false);

            return created;
        }
    }
}
