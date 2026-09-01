using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class MenuRoleService : IMenuRoleService
    {
        private readonly IMenuRoleRepository _repo;

        public MenuRoleService(IMenuRoleRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<MenuRole> AddAsync(MenuRole menuRole, CancellationToken cancellationToken = default)
            => _repo.AddAsync(menuRole, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<MenuRole>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<MenuRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(MenuRole menuRole, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(menuRole, cancellationToken);
    }
}