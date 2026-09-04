using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public interface IPurchaseReturnService
    {
        Task<PurchaseReturn?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PurchaseReturn>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<PurchaseReturn>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<PurchaseReturn> AddAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken = default);
        Task UpdateAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Purchase Return Details
        Task<IReadOnlyList<PurchaseReturnDetail>> GetPurchaseReturnDetailsAsync(int purchaseReturnId, CancellationToken cancellationToken = default);
        Task<PurchaseReturnDetail> AddPurchaseReturnDetailAsync(PurchaseReturnDetail detail, CancellationToken cancellationToken = default);
        Task UpdatePurchaseReturnDetailAsync(PurchaseReturnDetail detail, CancellationToken cancellationToken = default);
        Task DeletePurchaseReturnDetailAsync(int id, CancellationToken cancellationToken = default);

        // Purchase Return with details
        Task<PurchaseReturn> CreatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default);
        Task<PurchaseReturn> UpdatePurchaseReturnAsync(PurchaseReturn returnOrder, IEnumerable<PurchaseReturnDetail> details, CancellationToken cancellationToken = default);

        // Stock validation
        Task<int> GetAvailableStockAsync(int productId, CancellationToken cancellationToken = default);
        Task<bool> HasSufficientStockAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    }
}