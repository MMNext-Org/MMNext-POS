using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork, "Users")
        {
        }
    }
}