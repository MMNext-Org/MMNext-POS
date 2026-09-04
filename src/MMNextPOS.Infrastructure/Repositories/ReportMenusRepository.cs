using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class ReportMenusRepository : GenericRepository<ReportMenus>, IReportMenusRepository
    {
        public ReportMenusRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "ReportMenus")
        {
        }
    }
}
