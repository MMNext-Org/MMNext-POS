using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class PaymentVoucherRepository : GenericRepository<PaymentVoucher>, IPaymentVoucherRepository
    {
        public PaymentVoucherRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "PaymentVouchers")
        {
        }

        public async Task<PaymentVoucher?> GetByVoucherNoAsync(string voucherNo, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PaymentVouchers WHERE VoucherNo = @VoucherNo AND IsDeleted = 0";
            return await Connection.QuerySingleOrDefaultAsync<PaymentVoucher>(sql, new { VoucherNo = voucherNo }, Transaction).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PaymentVouchers WHERE VoucherDate >= @FromDate AND VoucherDate <= @ToDate AND IsDeleted = 0 ORDER BY VoucherDate DESC";
            var result = await Connection.QueryAsync<PaymentVoucher>(sql, new { FromDate = fromDate, ToDate = toDate }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PaymentVouchers WHERE CustomerId = @CustomerId AND IsDeleted = 0 ORDER BY VoucherDate DESC";
            var result = await Connection.QueryAsync<PaymentVoucher>(sql, new { CustomerId = customerId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PaymentVouchers WHERE SupplierId = @SupplierId AND IsDeleted = 0 ORDER BY VoucherDate DESC";
            var result = await Connection.QueryAsync<PaymentVoucher>(sql, new { SupplierId = supplierId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetBySaleAsync(int saleId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PaymentVouchers WHERE SaleId = @SaleId AND IsDeleted = 0 ORDER BY VoucherDate DESC";
            var result = await Connection.QueryAsync<PaymentVoucher>(sql, new { SaleId = saleId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<IReadOnlyList<PaymentVoucher>> GetByPurchaseAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PaymentVouchers WHERE PurchaseId = @PurchaseId AND IsDeleted = 0 ORDER BY VoucherDate DESC";
            var result = await Connection.QueryAsync<PaymentVoucher>(sql, new { PurchaseId = purchaseId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
