using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class RegistrationRepository : GenericRepository<Registration>, IRegistrationRepository
    {
        public RegistrationRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Registrations")
        {
        }
    }
}
