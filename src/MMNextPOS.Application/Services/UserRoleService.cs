using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _repo;

        public UserRoleService(IUserRoleRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<UserRole> AddAsync(UserRole userRole, CancellationToken cancellationToken = default)
            => _repo.AddAsync(userRole, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<UserRole>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<UserRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(UserRole userRole, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(userRole, cancellationToken);
    }
}