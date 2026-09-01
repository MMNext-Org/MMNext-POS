using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StockMovementDetailRepository : GenericRepository<StockMovementDetail>, IStockMovementDetailRepository
    {
        public StockMovementDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StockMovementDetails")
        {
        }
    }
}
