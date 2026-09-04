using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IPurchaseReceiptRepository : IRepository<PurchaseReceipt>
    {
        Task<PurchaseReceipt?> GetByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PurchaseReceipt>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<PurchaseReceipt> AddWithDetailsAsync(PurchaseReceipt receipt, IEnumerable<PurchaseReceiptDetail> details, CancellationToken cancellationToken = default);
    }

    public interface IPurchaseReceiptDetailRepository : IRepository<PurchaseReceiptDetail>
    {
        Task<IReadOnlyList<PurchaseReceiptDetail>> GetByReceiptIdAsync(int receiptId, CancellationToken cancellationToken = default);
    }
}