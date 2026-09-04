using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class CurrencyRepository : GenericRepository<Currency>, ICurrencyRepository
    {
        public CurrencyRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Currencies")
        {
        }
    }
}
