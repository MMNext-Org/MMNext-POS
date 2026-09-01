using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ICompanyService
    {
        Task<Company?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default);
        Task UpdateAsync(Company company, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}