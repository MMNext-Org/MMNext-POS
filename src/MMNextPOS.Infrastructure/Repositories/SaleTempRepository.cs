using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class SaleTempRepository : GenericRepository<SaleTemp>, ISaleTempRepository
    {
        public SaleTempRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork, "SaleTemps")
        {
        }
    }
}