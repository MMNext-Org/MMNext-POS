using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class PcClientRepository : GenericRepository<PcClient>, IPcClientRepository
    {
        public PcClientRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "PcClients")
        {
        }
    }
}
