using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class LocationRepository : GenericRepository<Location>, ILocationRepository
    {
        public LocationRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork, "Locations")
        {
        }
    }
}
