using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IPurchaseReceiptService
    {
        Task<PurchaseReceipt?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PurchaseReceipt?> GetByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PurchaseReceipt>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<PurchaseReceipt> CreateReceiptAsync(PurchaseReceipt receipt, IEnumerable<PurchaseReceiptDetail> details, CancellationToken cancellationToken = default);
        Task<PurchaseReceipt> UpdateReceiptAsync(PurchaseReceipt receipt, CancellationToken cancellationToken = default);
        Task DeleteReceiptAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PurchaseReceiptDetail>> GetReceiptDetailsAsync(int receiptId, CancellationToken cancellationToken = default);
    }
}