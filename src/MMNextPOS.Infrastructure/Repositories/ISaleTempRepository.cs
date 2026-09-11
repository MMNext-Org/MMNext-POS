using MMNextPOS.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISaleTempRepository : IRepository<SaleTemp>
    {
        Task<IReadOnlyList<SaleTemp>> GetDraftsByStatusAsync(string status, CancellationToken cancellationToken = default);
    }
}