using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        /// <summary>
        /// Gets all invoices for a specific sale.
        /// </summary>
        Task<IReadOnlyList<Invoice>> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
    }
}
