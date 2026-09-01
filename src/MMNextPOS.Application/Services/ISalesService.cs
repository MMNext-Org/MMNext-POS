using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISalesService
    {
        Task<Sale> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Sale>> GetRecentSalesAsync(int count = 20, CancellationToken cancellationToken = default);
        Task<SaleDetail> AddSaleDetailAsync(int saleId, SaleDetail detail, CancellationToken cancellationToken = default);
    }
}
