using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class LanguageRepository : GenericRepository<Language>, ILanguageRepository
    {
        public LanguageRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "Languages")
        {
        }
    }
}
