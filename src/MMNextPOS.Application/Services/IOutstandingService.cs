using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IOutstandingService
    {
        // Customer outstanding
        Task<IReadOnlyList<CustomerOutstanding>> GetCustomerOutstandingAsync(int customerId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CustomerOutstanding>> GetAllCustomerOutstandingAsync(CancellationToken cancellationToken = default);
        Task<CustomerOutstanding> AddCustomerOutstandingAsync(CustomerOutstanding outstanding, CancellationToken cancellationToken = default);
        Task UpdateCustomerOutstandingAsync(CustomerOutstanding outstanding, CancellationToken cancellationToken = default);
        Task DeleteCustomerOutstandingAsync(int id, CancellationToken cancellationToken = default);

        // Supplier outstanding
        Task<IReadOnlyList<SupplierOutstanding>> GetSupplierOutstandingAsync(int supplierId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SupplierOutstanding>> GetAllSupplierOutstandingAsync(CancellationToken cancellationToken = default);
        Task<SupplierOutstanding> AddSupplierOutstandingAsync(SupplierOutstanding outstanding, CancellationToken cancellationToken = default);
        Task UpdateSupplierOutstandingAsync(SupplierOutstanding outstanding, CancellationToken cancellationToken = default);
        Task DeleteSupplierOutstandingAsync(int id, CancellationToken cancellationToken = default);
    }
}
