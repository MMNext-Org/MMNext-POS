using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class SaleTempService : ISaleTempService
    {
        private readonly ISaleTempRepository _repo;

        public SaleTempService(ISaleTempRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<SaleTemp> AddAsync(SaleTemp saleTemp, CancellationToken cancellationToken = default)
            => _repo.AddAsync(saleTemp, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<SaleTemp>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<SaleTemp?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(SaleTemp saleTemp, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(saleTemp, cancellationToken);
    }
}