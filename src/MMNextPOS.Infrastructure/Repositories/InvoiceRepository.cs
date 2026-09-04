using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Invoices")
        {
        }
    }
}
