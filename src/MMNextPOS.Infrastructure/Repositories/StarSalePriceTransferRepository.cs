using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StarSalePriceTransferRepository : GenericRepository<StarSalePriceTransfer>, IStarSalePriceTransferRepository
    {
        public StarSalePriceTransferRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StarSalePriceTransfers")
        {
        }
    }
}
