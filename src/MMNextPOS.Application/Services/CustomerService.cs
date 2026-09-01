using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepo;

        public CustomerService(ICustomerRepository customerRepo)
        {
            _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
        }

        public Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            return _customerRepo.AddAsync(customer, cancellationToken);
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return _customerRepo.DeleteAsync(id, cancellationToken);
        }

        public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _customerRepo.GetAllAsync(cancellationToken);
        }

        public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _customerRepo.GetByIdAsync(id, cancellationToken);
        }

        public Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            return _customerRepo.UpdateAsync(customer, cancellationToken);
        }
    }
}
