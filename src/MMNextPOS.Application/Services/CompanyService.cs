using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _repo;

        public CompanyService(ICompanyRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default)
            => _repo.AddAsync(company, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<Company?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(company, cancellationToken);
    }
}