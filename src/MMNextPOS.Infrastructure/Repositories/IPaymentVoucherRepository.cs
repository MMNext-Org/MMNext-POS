using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IPaymentVoucherRepository : IRepository<PaymentVoucher>
    {
        Task<PaymentVoucher?> GetByVoucherNoAsync(string voucherNo, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetBySaleAsync(int saleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetByPurchaseAsync(int purchaseId, CancellationToken cancellationToken = default);
    }
}
