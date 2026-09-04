using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class ChangeDateLogRepository : GenericRepository<ChangeDateLog>, IChangeDateLogRepository
    {
        public ChangeDateLogRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "ChangeDateLogs")
        {
        }
    }
}
