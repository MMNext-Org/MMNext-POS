using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class UnitRepository : GenericRepository<Unit>, IUnitRepository
    {
        public UnitRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork, "Units")
        {
        }
    }
}
