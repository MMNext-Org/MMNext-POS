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
    public class PurchaseReceiptRepository : GenericRepository<PurchaseReceipt>, IPurchaseReceiptRepository
    {
        public PurchaseReceiptRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "PurchaseReceipts")
        {
        }

        public async Task<PurchaseReceipt?> GetByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PurchaseReceipts WHERE PurchaseId = @PurchaseId AND IsDeleted = 0 ORDER BY CreatedAt DESC LIMIT 1";
            return await Connection.QuerySingleOrDefaultAsync<PurchaseReceipt>(sql, new { PurchaseId = purchaseId }, Transaction).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<PurchaseReceipt>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PurchaseReceipts WHERE PurchaseDate >= @FromDate AND PurchaseDate <= @ToDate AND IsDeleted = 0 ORDER BY PurchaseDate DESC";
            var result = await Connection.QueryAsync<PurchaseReceipt>(sql, new { FromDate = fromDate, ToDate = toDate }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }

        public async Task<PurchaseReceipt> AddWithDetailsAsync(PurchaseReceipt receipt, IEnumerable<PurchaseReceiptDetail> details, CancellationToken cancellationToken = default)
        {
            var result = await AddAsync(receipt, cancellationToken).ConfigureAwait(false);

            foreach (var detail in details)
            {
                detail.PurchaseReceiptId = result.Id;
                const string sql = @"INSERT INTO PurchaseReceiptDetails (PurchaseReceiptId, ProductSku, ProductName, Quantity, ReceivedQuantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, UnitOfMeasure, BatchNumber, ExpiryDate)
                                    VALUES (@PurchaseReceiptId, @ProductSku, @ProductName, @Quantity, @ReceivedQuantity, @UnitPrice, @DiscountAmount, @TaxAmount, @LineTotal, @UnitOfMeasure, @BatchNumber, @ExpiryDate);
                                    SELECT LAST_INSERT_ID();";
                var id = await Connection.ExecuteScalarAsync<long>(sql, new
                {
                    PurchaseReceiptId = result.Id,
                    detail.ProductSku,
                    detail.ProductName,
                    detail.Quantity,
                    detail.ReceivedQuantity,
                    detail.UnitPrice,
                    detail.DiscountAmount,
                    detail.TaxAmount,
                    detail.LineTotal,
                    detail.UnitOfMeasure,
                    detail.BatchNumber,
                    detail.ExpiryDate
                }, Transaction).ConfigureAwait(false);
                detail.Id = (int)id;
            }

            return result;
        }
    }

    public class PurchaseReceiptDetailRepository : GenericRepository<PurchaseReceiptDetail>, IPurchaseReceiptDetailRepository
    {
        public PurchaseReceiptDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "PurchaseReceiptDetails")
        {
        }

        public async Task<IReadOnlyList<PurchaseReceiptDetail>> GetByReceiptIdAsync(int receiptId, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM PurchaseReceiptDetails WHERE PurchaseReceiptId = @ReceiptId";
            var result = await Connection.QueryAsync<PurchaseReceiptDetail>(sql, new { ReceiptId = receiptId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}