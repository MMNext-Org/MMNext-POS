using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public interface IPaymentService
    {
        Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<Payment>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);
        Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Payment filtering
        Task<IReadOnlyList<Payment>> GetByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Payment>> GetBySupplierAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    }
}
