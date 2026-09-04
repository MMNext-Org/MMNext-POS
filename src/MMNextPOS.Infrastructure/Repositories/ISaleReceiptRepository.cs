using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISaleReceiptRepository : IRepository<SaleReceipt>
    {
        Task<SaleReceipt?> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SaleReceipt>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<SaleReceipt> AddWithDetailsAsync(SaleReceipt receipt, IEnumerable<SaleReceiptDetail> details, CancellationToken cancellationToken = default);
    }

    public interface ISaleReceiptDetailRepository : IRepository<SaleReceiptDetail>
    {
        Task<IReadOnlyList<SaleReceiptDetail>> GetByReceiptIdAsync(int receiptId, CancellationToken cancellationToken = default);
    }
}