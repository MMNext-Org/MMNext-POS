using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StarOutstandingReportRepository : GenericRepository<StarOutstandingReport>, IStarOutstandingReportRepository
    {
        public StarOutstandingReportRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StarOutstandingReports")
        {
        }
    }
}
