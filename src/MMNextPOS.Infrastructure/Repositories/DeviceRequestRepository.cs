using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class DeviceRequestRepository : GenericRepository<DeviceRequest>, IDeviceRequestRepository
    {
        public DeviceRequestRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "DeviceRequests")
        {
        }
    }
}
