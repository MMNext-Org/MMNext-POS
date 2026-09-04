using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StarStockTransferReceivedRepository : GenericRepository<StarStockTransferReceived>, IStarStockTransferReceivedRepository
    {
        public StarStockTransferReceivedRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StarStockTransferReceiveds")
        {
        }
    }
}
