using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IStockTransferService
    {
        Task<StockTransfer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<StockTransfer> CreateTransferAsync(StockTransfer transfer, IEnumerable<StockTransferDetail> details, int userId, CancellationToken cancellationToken = default);
        Task<StockTransfer> ReleaseTransferAsync(int transferId, int userId, CancellationToken cancellationToken = default);
        Task<StockTransfer> ReceiveTransferAsync(int transferId, IEnumerable<(int detailId, int receivedQuantity, string? serialNumber)> receivedItems, int receivedByUserId, CancellationToken cancellationToken = default);
        Task<StockTransfer> CancelTransferAsync(int transferId, int userId, string? reason, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StockTransfer>> GetTransfersAsync(int? fromLocationId = null, int? toLocationId = null, string? status = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StockTransfer>> GetInTransitTransfersAsync(int? toLocationId = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StockTransferDetail>> GetTransferDetailsAsync(int transferId, CancellationToken cancellationToken = default);
        Task<decimal> GetTransferCostAsync(int transferId, CancellationToken cancellationToken = default);
        Task<string> GenerateTransferNumberAsync(CancellationToken cancellationToken = default);
    }
}
