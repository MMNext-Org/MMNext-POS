using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class ExpenseTypeRepository : GenericRepository<ExpenseType>, IExpenseTypeRepository
    {
        public ExpenseTypeRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "ExpenseTypes")
        {
        }
    }
}
