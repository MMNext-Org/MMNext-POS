using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IGroupService
    {
        Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Group> AddAsync(Group group, CancellationToken cancellationToken = default);
        Task UpdateAsync(Group group, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
