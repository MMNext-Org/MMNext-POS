using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class DeviceInfoRepository : GenericRepository<DeviceInfo>, IDeviceInfoRepository
    {
        public DeviceInfoRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "DeviceInfos")
        {
        }
    }
}
