using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class PCUpdateRepository : GenericRepository<PCUpdate>, IPCUpdateRepository
    {
        public PCUpdateRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "PCUpdates")
        {
        }
    }
}
