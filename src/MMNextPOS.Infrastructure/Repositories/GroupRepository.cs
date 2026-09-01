using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class GroupRepository : GenericRepository<Group>, IGroupRepository
    {
        public GroupRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork, "Groups")
        {
        }
    }
}