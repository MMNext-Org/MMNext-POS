using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class PurchaseRepository : GenericRepository<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Purchases")
        {
        }
    }
}
