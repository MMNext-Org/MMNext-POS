using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Roles")
        {
        }
    }
}
