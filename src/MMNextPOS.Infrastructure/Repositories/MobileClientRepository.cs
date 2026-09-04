using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class MobileClientRepository : GenericRepository<MobileClient>, IMobileClientRepository
    {
        public MobileClientRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "MobileClients")
        {
        }
    }
}
