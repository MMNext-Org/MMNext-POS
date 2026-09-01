using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StockTransferDetailRepository : GenericRepository<StockTransferDetail>, IStockTransferDetailRepository
    {
        public StockTransferDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StockTransferDetails")
        {
        }
    }
}
