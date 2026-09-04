using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISupplierService
    {
        Task<Supplier?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
