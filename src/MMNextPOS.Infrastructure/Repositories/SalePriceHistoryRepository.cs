using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SalePriceHistoryRepository : GenericRepository<SalePriceHistory>, ISalePriceHistoryRepository
    {
        public SalePriceHistoryRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "SalePriceHistories")
        {
        }
    }
}
