using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IAssemblyService
    {
        // Assembly CRUD
        Task<Assembly?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Assembly>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Assembly> CreateAsync(Assembly assembly, IEnumerable<AssemblyDetail> components, int userId, CancellationToken cancellationToken = default);
        Task<Assembly> UpdateAsync(Assembly assembly, IEnumerable<AssemblyDetail> components, int userId, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);

        // Assembly Operations
        Task<Assembly> BuildAssemblyAsync(int assemblyId, int quantity, int userId, CancellationToken cancellationToken = default);
        Task<Assembly> DeassembleAsync(int assemblyId, int quantity, int userId, CancellationToken cancellationToken = default);

        // Queries
        Task<IReadOnlyList<AssemblyDetail>> GetAssemblyDetailsAsync(int assemblyId, CancellationToken cancellationToken = default);
        Task<decimal> CalculateAssemblyCostAsync(int assemblyId, CancellationToken cancellationToken = default);
        Task<AssemblyCostVariance> GetCostVarianceAsync(int assemblyId, CancellationToken cancellationToken = default);
    }
}

// DTO for cost variance reporting
namespace MMNextPOS.Application.Services
{
    public class AssemblyCostVariance
    {
        public int AssemblyId { get; set; }
        public decimal StandardCost { get; set; }
        public decimal ActualCost { get; set; }
        public decimal Variance => ActualCost - StandardCost;
        public decimal VariancePercentage => StandardCost != 0 ? (Variance / StandardCost) * 100m : 0m;
    }
}
