using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
            => _repo.AddAsync(category, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(category, cancellationToken);
    }
}