using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class ClientUpdateRequestRepository : GenericRepository<ClientUpdateRequest>, IClientUpdateRequestRepository
    {
        public ClientUpdateRequestRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "ClientUpdateRequests")
        {
        }
    }
}
