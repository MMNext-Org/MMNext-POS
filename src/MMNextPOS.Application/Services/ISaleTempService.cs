using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISaleTempService
    {
        Task<SaleTemp?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SaleTemp>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SaleTemp> AddAsync(SaleTemp saleTemp, CancellationToken cancellationToken = default);
        Task UpdateAsync(SaleTemp saleTemp, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
