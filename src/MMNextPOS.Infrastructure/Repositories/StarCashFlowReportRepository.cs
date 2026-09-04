using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StarCashFlowReportRepository : GenericRepository<StarCashFlowReport>, IStarCashFlowReportRepository
    {
        public StarCashFlowReportRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StarCashFlowReports")
        {
        }
    }
}
