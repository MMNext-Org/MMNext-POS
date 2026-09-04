using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class MenuRoleRepository : GenericRepository<MenuRole>, IMenuRoleRepository
    {
        public MenuRoleRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "MenuRoles")
        {
        }
    }
}
