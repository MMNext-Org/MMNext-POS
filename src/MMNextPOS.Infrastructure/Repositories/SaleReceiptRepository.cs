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
    public class SaleReceiptRepository : GenericRepository<SaleReceipt>, ISaleReceiptRepository
    {
        public SaleReceiptRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SaleReceipts")
        {
        }

        public async Task<SaleReceipt?> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM SaleReceipts WHERE SaleId = @SaleId AND IsDeleted = 0 ORDER BY CreatedAt DESC LIMIT 1";
            return await Connection.QuerySingleOrDefaultAsync<SaleReceipt>(sql, new { SaleId = saleId }, Transaction).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<SaleReceipt>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM SaleReceipts WHERE SaleDate >= @FromDate AND SaleDate <= @ToDate AND IsDeleted = 0 ORDER BY SaleDate DESC";
            var result = await Connection.QueryAsync<SaleReceipt>(sql, new { FromDate = fromDate, ToDate = toDate }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<SaleReceipt> AddWithDetailsAsync(SaleReceipt receipt, IEnumerable<SaleReceiptDetail> details, CancellationToken cancellationToken = default)
        {
            // Add receipt header
            var result = await AddAsync(receipt, cancellationToken).ConfigureAwait(false);

            // Add details
            foreach (var detail in details)
            {
                detail.SaleReceiptId = result.Id;
                const string sql = @"INSERT INTO SaleReceiptDetails (SaleReceiptId, ProductSku, ProductName, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, UnitOfMeasure)
                                    VALUES (@SaleReceiptId, @ProductSku, @ProductName, @Quantity, @UnitPrice, @DiscountAmount, @TaxAmount, @LineTotal, @UnitOfMeasure);
                                    SELECT LAST_INSERT_ID();";
                var id = await Connection.ExecuteScalarAsync<long>(sql, new
                {
                    SaleReceiptId = result.Id,
                    detail.ProductSku,
                    detail.ProductName,
                    detail.Quantity,
                    detail.UnitPrice,
                    detail.DiscountAmount,
                    detail.TaxAmount,
                    detail.LineTotal,
                    detail.UnitOfMeasure
                }, Transaction).ConfigureAwait(false);
                detail.Id = (int)id;
            }

            return result;
        }
    }

    public class SaleReceiptDetailRepository : GenericRepository<SaleReceiptDetail>, ISaleReceiptDetailRepository
    {
        public SaleReceiptDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SaleReceiptDetails")
        {
        }

        public async Task<IReadOnlyList<SaleReceiptDetail>> GetByReceiptIdAsync(int receiptId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM SaleReceiptDetails WHERE SaleReceiptId = @ReceiptId";
            var result = await Connection.QueryAsync<SaleReceiptDetail>(sql, new { ReceiptId = receiptId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
