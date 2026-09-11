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

        /// <summary>
        /// Gets drafts filtered by status (e.g., "Draft", "Finalized", "Voided").
        /// </summary>
        /// <param name="status">The status to filter by.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of sale temp drafts.</returns>
        Task<IReadOnlyList<SaleTemp>> GetDraftsByStatusAsync(string status, CancellationToken cancellationToken = default);
    }
}
