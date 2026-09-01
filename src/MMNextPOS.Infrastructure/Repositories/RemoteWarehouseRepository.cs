using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class RemoteWarehouseRepository : GenericRepository<RemoteWarehouse>, IRemoteWarehouseRepository
    {
        public RemoteWarehouseRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "RemoteWarehouses")
        {
        }
    }
}
