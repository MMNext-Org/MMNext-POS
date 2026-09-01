using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MMNextPOS.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository interface for all entities.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
