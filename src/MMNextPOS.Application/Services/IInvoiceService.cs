using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IInvoiceService
    {
        Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
        Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
