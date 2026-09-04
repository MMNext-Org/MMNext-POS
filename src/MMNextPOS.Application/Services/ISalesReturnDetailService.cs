using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public interface ISalesReturnDetailService
    {
        Task<IReadOnlyList<SalesReturnDetail>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<SalesReturnDetail?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReturnDetail> AddAsync(SalesReturnDetail salesReturnDetail, CancellationToken cancellationToken = default);
        Task UpdateAsync(SalesReturnDetail salesReturnDetail, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
