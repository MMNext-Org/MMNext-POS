using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public interface IPurchaseService
    {
        Task<Purchase?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Purchase>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<Purchase>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Purchase> AddAsync(Purchase purchase, CancellationToken cancellationToken = default);
        Task UpdateAsync(Purchase purchase, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Purchase Details
        Task<IReadOnlyList<PurchaseDetail>> GetPurchaseDetailsAsync(int purchaseId, CancellationToken cancellationToken = default);
        Task<PurchaseDetail> AddPurchaseDetailAsync(PurchaseDetail detail, CancellationToken cancellationToken = default);
        Task UpdatePurchaseDetailAsync(PurchaseDetail detail, CancellationToken cancellationToken = default);
        Task DeletePurchaseDetailAsync(int id, CancellationToken cancellationToken = default);

        // Purchase with details
        Task<Purchase> CreatePurchaseWithDetailsAsync(Purchase purchase, IEnumerable<PurchaseDetail> details, CancellationToken cancellationToken = default);
        Task<Purchase> UpdatePurchaseWithDetailsAsync(Purchase purchase, IEnumerable<PurchaseDetail> details, CancellationToken cancellationToken = default);

        // Purchase lifecycle
        Task<Purchase> ReceivePurchaseAsync(int purchaseId, int receivedByUserId, IEnumerable<(int detailId, int receivedQuantity)> receivedItems, CancellationToken cancellationToken = default);
        Task<Purchase> HoldPurchaseAsync(int purchaseId, int userId, string? reason = null, CancellationToken cancellationToken = default);
        Task<Purchase> ReleasePurchaseAsync(int purchaseId, int userId, CancellationToken cancellationToken = default);
        Task<Purchase> CancelPurchaseAsync(int purchaseId, int userId, string? reason = null, CancellationToken cancellationToken = default);

        // Purchase Returns
        Task<PurchaseReturn?> GetPurchaseReturnByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PurchaseReturn>> GetPurchaseReturnsAsync(int? supplierId = null, string? status = null, CancellationToken cancellationToken = default);
        Task<PurchaseReturn> CreatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default);
        Task<PurchaseReturn> UpdatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default);
        Task<PurchaseReturn> ReceivePurchaseReturnAsync(int returnId, int receivedByUserId, IEnumerable<(int detailId, int receivedQuantity)> receivedItems, CancellationToken cancellationToken = default);
        Task DeletePurchaseReturnAsync(int id, CancellationToken cancellationToken = default);

        // Stock validation
        Task<int> GetAvailableStockAsync(int productId, CancellationToken cancellationToken = default);
        Task<bool> HasSufficientStockAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    }
}