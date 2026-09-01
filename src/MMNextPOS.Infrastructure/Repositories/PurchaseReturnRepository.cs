using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class PurchaseReturnRepository : GenericRepository<PurchaseReturn>, IPurchaseReturnRepository
    {
        public PurchaseReturnRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "PurchaseReturns")
        {
        }
    }
}
