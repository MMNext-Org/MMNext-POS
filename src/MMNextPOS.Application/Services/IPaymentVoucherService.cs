using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IPaymentVoucherService
    {
        Task<PaymentVoucher?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PaymentVoucher?> GetByVoucherNoAsync(string voucherNo, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetBySaleAsync(int saleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentVoucher>> GetByPurchaseAsync(int purchaseId, CancellationToken cancellationToken = default);
        Task<PaymentVoucher> AddAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);
        Task UpdateAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}