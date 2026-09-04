using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public interface IInventoryService
    {
        // Stock Movement
        Task<StockMovement?> GetStockMovementByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StockMovement>> GetStockMovementsAsync(int? locationId = null, string? movementType = null, CancellationToken cancellationToken = default);
        Task<PagedResult<StockMovement>> GetStockMovementsPageAsync(int page, int pageSize, int? locationId = null, string? movementType = null, CancellationToken cancellationToken = default);
        Task<StockMovement> AddStockMovementAsync(StockMovement movement, IEnumerable<StockMovementDetail> details, CancellationToken cancellationToken = default);
        Task UpdateStockMovementAsync(StockMovement movement, IEnumerable<StockMovementDetail> details, CancellationToken cancellationToken = default);
        Task DeleteStockMovementAsync(int id, CancellationToken cancellationToken = default);

        // Stock Movement Details
        Task<IReadOnlyList<StockMovementDetail>> GetStockMovementDetailsAsync(int stockMovementId, CancellationToken cancellationToken = default);
        Task<StockMovementDetail> AddStockMovementDetailAsync(StockMovementDetail detail, CancellationToken cancellationToken = default);
        Task UpdateStockMovementDetailAsync(StockMovementDetail detail, CancellationToken cancellationToken = default);
        Task DeleteStockMovementDetailAsync(int id, CancellationToken cancellationToken = default);

        // Assembly (BOM)
        Task<Assembly?> GetAssemblyByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Assembly>> GetAssembliesAsync(CancellationToken cancellationToken = default);
        Task<Assembly> AddAssemblyAsync(Assembly assembly, IEnumerable<AssemblyDetail> details, CancellationToken cancellationToken = default);
        Task UpdateAssemblyAsync(Assembly assembly, IEnumerable<AssemblyDetail> details, CancellationToken cancellationToken = default);
        Task DeleteAssemblyAsync(int id, CancellationToken cancellationToken = default);

        // Assembly Details
        Task<IReadOnlyList<AssemblyDetail>> GetAssemblyDetailsAsync(int assemblyId, CancellationToken cancellationToken = default);
        Task<AssemblyDetail> AddAssemblyDetailAsync(AssemblyDetail detail, CancellationToken cancellationToken = default);
        Task UpdateAssemblyDetailAsync(AssemblyDetail detail, CancellationToken cancellationToken = default);
        Task DeleteAssemblyDetailAsync(int id, CancellationToken cancellationToken = default);

        // Stock Transfers
        Task<StockTransfer?> GetStockTransferByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StockTransfer>> GetStockTransfersAsync(int? fromLocationId = null, int? toLocationId = null, string? status = null, CancellationToken cancellationToken = default);
        Task<PagedResult<StockTransfer>> GetStockTransfersPageAsync(int page, int pageSize, int? fromLocationId = null, int? toLocationId = null, string? status = null, CancellationToken cancellationToken = default);
        Task<StockTransfer> AddStockTransferAsync(StockTransfer transfer, IEnumerable<StockTransferDetail> details, CancellationToken cancellationToken = default);
        Task UpdateStockTransferAsync(StockTransfer transfer, IEnumerable<StockTransferDetail> details, CancellationToken cancellationToken = default);
        Task ReceiveStockTransferAsync(int transferId, int receivedByUserId, IEnumerable<(int detailId, int receivedQuantity, string? serialNumber)> receivedItems, CancellationToken cancellationToken = default);

        // Serial Number Tracking
        Task<IReadOnlyList<StockMovementDetail>> GetSerialTrackedItemsAsync(int productId, int? locationId = null, CancellationToken cancellationToken = default);
        Task<StockMovementDetail?> GetSerialDetailAsync(string serialNumber, CancellationToken cancellationToken = default);

        // Stock Validation & Queries
        Task<int> GetAvailableStockAsync(int productId, int? locationId = null, CancellationToken cancellationToken = default);
        Task<bool> HasSufficientStockAsync(int productId, int quantity, int? locationId = null, CancellationToken cancellationToken = default);
        Task<decimal> GetAverageCostAsync(int productId, CancellationToken cancellationToken = default);
    }
}