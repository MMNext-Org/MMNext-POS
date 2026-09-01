using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _repo;

        public GroupService(IGroupRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<Group> AddAsync(Group group, CancellationToken cancellationToken = default)
            => _repo.AddAsync(group, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(Group group, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(group, cancellationToken);
    }
}