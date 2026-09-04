using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class StarStockBalanceReportRepository : GenericRepository<StarStockBalanceReport>, IStarStockBalanceReportRepository
    {
        public StarStockBalanceReportRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "StarStockBalanceReports")
        {
        }
    }
}
