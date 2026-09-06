using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISaleReceiptService
    {
        Task<SaleReceipt?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SaleReceipt?> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SaleReceipt>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<SaleReceipt> CreateReceiptAsync(SaleReceipt receipt, IEnumerable<SaleReceiptDetail> details, CancellationToken cancellationToken = default);
        Task<SaleReceipt> UpdateReceiptAsync(SaleReceipt receipt, CancellationToken cancellationToken = default);
        Task DeleteReceiptAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SaleReceiptDetail>> GetReceiptDetailsAsync(int receiptId, CancellationToken cancellationToken = default);
    }
}
