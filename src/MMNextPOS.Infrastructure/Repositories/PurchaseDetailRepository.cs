using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class PurchaseDetailRepository : GenericRepository<PurchaseDetail>, IPurchaseDetailRepository
    {
        public PurchaseDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "PurchaseDetails")
        {
        }
    }
}
