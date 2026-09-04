using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Suppliers")
        {
        }
    }
}
