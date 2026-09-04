using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class ThemeRepository : GenericRepository<Theme>, IThemeRepository
    {
        public ThemeRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Themes")
        {
        }
    }
}
