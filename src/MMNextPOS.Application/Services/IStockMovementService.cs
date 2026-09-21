using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Creates stock movement records (header + detail lines) inside the
    /// active <see cref="MMNextPOS.Infrastructure.IUnitOfWork"/> transaction.
    /// Used by sales, purchases, returns, adjustments, transfers, assemblies,
    /// and all other inventory operations to keep an auditable history of every
    /// quantity change.
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

        /// <summary>
        /// Persist a stock movement header and one detail row per purchase line.
        /// MovementType is "Purchase". Returns the created movement header with
        /// its assigned Id. The header records the SupplierId from the purchase
        /// so the supplier linkage is preserved on every movement row.
        /// </summary>
        Task<StockMovement> AddPurchaseMovementAsync(
            Purchase purchase,
            IReadOnlyList<PurchaseDetail> details,
            int? createdByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist a stock movement header and one detail row per return line.
        /// MovementType is "Return". Returns the created movement header with
        /// its assigned Id.
        /// </summary>
        Task<StockMovement> AddReturnMovementAsync(
            int returnId,
            int productId,
            int quantity,
            decimal unitCost,
            int? locationId,
            int? createdByUserId,
            string? reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist a stock movement header for a void operation.
        /// MovementType is "Void". Returns the created movement header with
        /// its assigned Id.
        /// </summary>
        Task<StockMovement> AddVoidMovementAsync(
            int saleId,
            int productId,
            int quantity,
            decimal unitCost,
            int? locationId,
            int? createdByUserId,
            string? reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist a stock movement for a manual issue (e.g., internal use, production consumption).
        /// MovementType is "Issue". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddIssueMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string reasonCode,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist a stock movement for a receipt (e.g., purchase receipt, return receipt, found stock).
        /// MovementType is "Receive". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddReceiveMovementAsync(
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
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist a stock movement for damaged goods write-off.
        /// MovementType is "Damaged". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddDamagedMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist a stock movement for lost/missing stock write-off.
        /// MovementType is "Lost". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddLostMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist a stock movement for a manual adjustment (positive or negative).
        /// MovementType is "Adjust". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddAdjustmentMovementAsync(
            int? locationId,
            int productId,
            int quantity, // Positive = increase, negative = decrease
            decimal unitCost,
            string reasonCode,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            string? referenceType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist stock movement for transfer out (source location).
        /// MovementType is "TransferOut". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddTransferOutMovementAsync(
            int fromLocationId,
            int toLocationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int transferId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist stock movement for transfer in (destination location).
        /// MovementType is "TransferIn". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddTransferInMovementAsync(
            int fromLocationId,
            int toLocationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int transferId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist stock movement for assembly build (components consumed).
        /// MovementType is "Assembly". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddAssemblyMovementAsync(
            int assemblyId,
            IReadOnlyList<AssemblyDetail> components,
            int? createdByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist stock movement for deassembly (components recovered).
        /// MovementType is "Deassembly". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddDeassemblyMovementAsync(
            int assemblyId,
            IReadOnlyList<AssemblyDetail> components,
            int? createdByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist stock movement for expired goods write-off.
        /// MovementType is "Expired". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddExpiredMovementAsync(
            int? locationId,
            int productId,
            int quantity,
            decimal unitCost,
            string? batchNumber,
            DateTime? expiryDate,
            int? createdByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Persist stock movement for cycle count adjustment.
        /// MovementType is "CycleCount". Returns the created movement header.
        /// </summary>
        Task<StockMovement> AddCycleCountMovementAsync(
            int? locationId,
            int productId,
            int countedQuantity,
            int systemQuantity,
            decimal unitCost,
            string? reason,
            int? createdByUserId,
            int? referenceId,
            CancellationToken cancellationToken = default);
    }
}
