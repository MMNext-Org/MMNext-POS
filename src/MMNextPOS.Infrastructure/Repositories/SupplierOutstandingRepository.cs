using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SupplierOutstandingRepository : GenericRepository<SupplierOutstanding>, ISupplierOutstandingRepository
    {
        public SupplierOutstandingRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SupplierOutstandings")
        {
        }
    }
}
