using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        public CompanyRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Companies")
        {
        }
    }
}
