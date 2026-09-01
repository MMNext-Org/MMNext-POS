using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class LinkedStockRepository : GenericRepository<LinkedStock>, ILinkedStockRepository
    {
        public LinkedStockRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "LinkedStocks")
        {
        }
    }
}
