using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _repo;

        public LocationService(ILocationRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default)
            => _repo.AddAsync(location, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(Location location, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(location, cancellationToken);
    }
}