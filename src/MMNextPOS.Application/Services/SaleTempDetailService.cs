using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Service for SaleTempDetail entities.
    /// </summary>
    public class SaleTempDetailService : ISaleTempDetailService
    {
        private readonly ISaleTempDetailRepository _repo;

        public SaleTempDetailService(ISaleTempDetailRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<SaleTempDetail>> GetBySaleTempIdAsync(int saleTempId, CancellationToken cancellationToken = default)
            => _repo.GetBySaleTempIdAsync(saleTempId, cancellationToken);

        /// <inheritdoc />
        public Task DeleteBySaleTempIdAsync(int saleTempId, CancellationToken cancellationToken = default)
            => _repo.DeleteBySaleTempIdAsync(saleTempId, cancellationToken);

        /// <inheritdoc />
        public Task<SaleTempDetail> AddAsync(SaleTempDetail detail, CancellationToken cancellationToken = default)
            => _repo.AddAsync(detail, cancellationToken);
    }
}
