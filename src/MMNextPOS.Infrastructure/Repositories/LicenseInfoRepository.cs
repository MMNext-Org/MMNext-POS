using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class LicenseInfoRepository : GenericRepository<LicenseInfo>, ILicenseInfoRepository
    {
        public LicenseInfoRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "LicenseInfos")
        {
        }
    }
}
