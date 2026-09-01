using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StockTransferRepository : GenericRepository<StockTransfer>, IStockTransferRepository
    {
        public StockTransferRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StockTransfers")
        {
        }
    }
}
