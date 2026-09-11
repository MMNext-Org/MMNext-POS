using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Invoices")
        {
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Invoice>> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
        {
            const string sql = @"SELECT * FROM Invoices WHERE SaleId = @SaleId AND IsDeleted = 0 ORDER BY InvoiceDate DESC";
            var result = await Connection.QueryAsync<Invoice>(sql, new { SaleId = saleId }, Transaction).ConfigureAwait(false);
            return result.AsList();
        }
    }
}
