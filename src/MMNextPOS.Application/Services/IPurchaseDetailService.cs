using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IPurchaseDetailService
    {
        Task<PurchaseDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PurchaseDetail>> GetByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default);
        Task<PurchaseDetail> AddAsync(PurchaseDetail detail, CancellationToken cancellationToken = default);
        Task UpdateAsync(PurchaseDetail detail, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
