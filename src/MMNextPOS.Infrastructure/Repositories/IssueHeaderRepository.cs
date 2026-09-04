using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class IssueHeaderRepository : GenericRepository<IssueHeader>, IIssueHeaderRepository
    {
        public IssueHeaderRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "IssueHeaders")
        {
        }
    }
}
