using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISerialNumberService
    {
        Task<SerialNumber> GenerateSerialAsync(int productId, int locationId, string? batchNumber = null, CancellationToken cancellationToken = default);
        Task<SerialNumber?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialNumber>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialNumber>> GetAvailableByProductAsync(int productId, int? locationId, CancellationToken cancellationToken = default);
        Task<SerialNumber> AssignToSaleAsync(string serialNumber, int saleId, int saleDetailId, int userId, CancellationToken cancellationToken = default);
        Task<SerialNumber> ReturnAsync(string serialNumber, int returnId, int userId, string? reason, CancellationToken cancellationToken = default);
        Task<SerialNumber> TransferAsync(string serialNumber, int fromLocationId, int toLocationId, int transferId, int userId, CancellationToken cancellationToken = default);
        Task<SerialNumber> MarkExpiredAsync(string serialNumber, int userId, CancellationToken cancellationToken = default);
        Task<SerialNumber> MarkDamagedAsync(string serialNumber, string reason, int userId, CancellationToken cancellationToken = default);
        Task<bool> ValidateSerialAsync(string serialNumber, int productId, CancellationToken cancellationToken = default);
        Task<string> GenerateSerialNumberAsync(int productId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SerialNumber>> GetAllSerialsAsync(int? locationId = null, CancellationToken cancellationToken = default);
    }
}
