using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Payments")
        {
        }
    }
}
