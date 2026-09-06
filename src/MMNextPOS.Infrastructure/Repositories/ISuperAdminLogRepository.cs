using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface ISuperAdminLogRepository : IRepository<SuperAdminLog>
    {
        Task<IReadOnlyList<SuperAdminLog>> GetByUserAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SuperAdminLog>> GetByModuleAsync(string module, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SuperAdminLog>> GetByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SuperAdminLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    }
}
