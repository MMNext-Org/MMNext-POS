using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IIssueHeaderService
    {
        Task<IssueHeader?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<IssueHeader>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IssueHeader> AddAsync(IssueHeader entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(IssueHeader entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
