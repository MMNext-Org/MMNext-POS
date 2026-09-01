using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class TaxRepository : GenericRepository<Tax>, ITaxRepository
    {
        public TaxRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork, "Taxes")
        {
        }
    }
}