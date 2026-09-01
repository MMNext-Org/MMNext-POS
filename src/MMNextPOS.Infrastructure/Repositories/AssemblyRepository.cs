using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class AssemblyRepository : GenericRepository<Assembly>, IAssemblyRepository
    {
        public AssemblyRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Assemblies")
        {
        }
    }
}
