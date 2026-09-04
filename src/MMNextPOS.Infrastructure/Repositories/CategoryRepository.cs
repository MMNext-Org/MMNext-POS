using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Categories")
        {
        }
    }
}
