using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Service interface for SaleTempDetail entities.
    /// </summary>
    public interface ISaleTempDetailService
    {
        Task<IReadOnlyList<SaleTempDetail>> GetBySaleTempIdAsync(int saleTempId, CancellationToken cancellationToken = default);
        Task DeleteBySaleTempIdAsync(int saleTempId, CancellationToken cancellationToken = default);
        Task<SaleTempDetail> AddAsync(SaleTempDetail detail, CancellationToken cancellationToken = default);
    }
}
