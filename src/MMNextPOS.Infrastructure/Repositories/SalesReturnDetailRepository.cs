using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SalesReturnDetailRepository : GenericRepository<SalesReturnDetail>, ISalesReturnDetailRepository
    {
        public SalesReturnDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SalesReturnDetails")
        {
        }
    }
}
