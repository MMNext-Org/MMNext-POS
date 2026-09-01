using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class AssemblyDetailRepository : GenericRepository<AssemblyDetail>, IAssemblyDetailRepository
    {
        public AssemblyDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "AssemblyDetails")
        {
        }
    }
}
