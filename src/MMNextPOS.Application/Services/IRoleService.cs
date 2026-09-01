using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IRoleService
    {
        Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);
        Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}