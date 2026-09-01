using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Expenses")
        {
        }
    }
}
