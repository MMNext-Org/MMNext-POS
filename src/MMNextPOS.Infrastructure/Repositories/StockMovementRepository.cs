using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StockMovementRepository : GenericRepository<StockMovement>, IStockMovementRepository
    {
        public StockMovementRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StockMovements")
        {
        }
    }
}
