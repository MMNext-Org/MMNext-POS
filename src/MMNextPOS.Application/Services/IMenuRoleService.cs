using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IMenuRoleService
    {
        Task<MenuRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MenuRole>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MenuRole> AddAsync(MenuRole menuRole, CancellationToken cancellationToken = default);
        Task UpdateAsync(MenuRole menuRole, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}