using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IReportMenusService
    {
        Task<ReportMenus?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReportMenus>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReportMenus>> GetByParentCodeAsync(string parentCode, CancellationToken cancellationToken = default);
        Task<ReportMenus> AddAsync(ReportMenus entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(ReportMenus entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
