using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StarReorderReportRepository : GenericRepository<StarReorderReport>, IStarReorderReportRepository
    {
        public StarReorderReportRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StarReorderReports")
        {
        }
    }
}
