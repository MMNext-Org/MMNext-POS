using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Journal voucher (double-entry accounting) service.
    /// </summary>
    public interface IJournalVoucherService
    {
        // Header CRUD
        Task<JournalVoucher?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalVoucher>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<JournalVoucher>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<JournalVoucher> AddAsync(JournalVoucher voucher, CancellationToken cancellationToken = default);
        Task UpdateAsync(JournalVoucher voucher, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Workflow
        Task<JournalVoucher> PostAsync(int id, CancellationToken cancellationToken = default);
        Task<JournalVoucher> VoidAsync(int id, CancellationToken cancellationToken = default);
    }
}
