using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class DiscountRepository : GenericRepository<Discount>, IDiscountRepository
    {
        public DiscountRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Discounts")
        {
        }
    }
}
