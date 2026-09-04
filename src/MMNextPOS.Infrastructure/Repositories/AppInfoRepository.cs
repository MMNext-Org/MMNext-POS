using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class AppInfoRepository : GenericRepository<AppInfo>, IAppInfoRepository
    {
        public AppInfoRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "AppInfos")
        {
        }
    }
}
