using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class CustomerOutstandingRepository : GenericRepository<CustomerOutstanding>, ICustomerOutstandingRepository
    {
        public CustomerOutstandingRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "CustomerOutstandings")
        {
        }
    }
}
