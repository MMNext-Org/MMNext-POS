using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StarProfitLossReportRepository : GenericRepository<StarProfitLossReport>, IStarProfitLossReportRepository
    {
        public StarProfitLossReportRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StarProfitLossReports")
        {
        }
    }
}
