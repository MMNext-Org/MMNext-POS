using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SalesReturnRepository : GenericRepository<SalesReturn>, ISalesReturnRepository
    {
        public SalesReturnRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SalesReturns")
        {
        }
    }
}
