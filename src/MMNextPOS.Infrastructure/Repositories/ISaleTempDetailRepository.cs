using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for SaleTempDetail entities.
    /// </summary>
    public interface ISaleTempDetailRepository : IRepository<SaleTempDetail>
    {
        /// <summary>
        /// Gets all details for a specific SaleTemp.
        /// </summary>
        Task<IReadOnlyList<SaleTempDetail>> GetBySaleTempIdAsync(int saleTempId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all details for a specific SaleTemp.
        /// </summary>
        Task DeleteBySaleTempIdAsync(int saleTempId, CancellationToken cancellationToken = default);
    }
}