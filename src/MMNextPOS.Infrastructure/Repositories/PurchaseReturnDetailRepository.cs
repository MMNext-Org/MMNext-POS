using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class PurchaseReturnDetailRepository : GenericRepository<PurchaseReturnDetail>, IPurchaseReturnDetailRepository
    {
        public PurchaseReturnDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "PurchaseReturnDetails")
        {
        }
    }
}
