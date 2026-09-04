using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IUserRoleService
    {
        Task<UserRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserRole>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserRole> AddAsync(UserRole userRole, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserRole userRole, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
