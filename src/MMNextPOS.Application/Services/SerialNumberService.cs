using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.Infrastructure;

namespace MMNextPOS.Application.Services
{
    public class SerialNumberService : ISerialNumberService
    {
        private readonly ISerialNumberRepository _serialNumberRepo;
        private readonly ISerialBatchRepository _batchRepo;
        private readonly ISerialTrackingRepository _trackingRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public SerialNumberService(
            ISerialNumberRepository serialNumberRepo,
            ISerialBatchRepository batchRepo,
            ISerialTrackingRepository trackingRepo,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _serialNumberRepo = serialNumberRepo ?? throw new ArgumentNullException(nameof(serialNumberRepo));
            _batchRepo = batchRepo ?? throw new ArgumentNullException(nameof(batchRepo));
            _trackingRepo = trackingRepo ?? throw new ArgumentNullException(nameof(trackingRepo));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<SerialNumber> GenerateSerialAsync(int productId, int locationId, string? batchNumber = null, CancellationToken cancellationToken = default)
        {
            var serialValue = await GenerateSerialNumberAsync(productId, cancellationToken);

            int? batchId = null;
            if (!string.IsNullOrEmpty(batchNumber))
            {
                var batch = await _batchRepo.GetByBatchNumberAsync(batchNumber, cancellationToken);
                batchId = batch?.Id;
            }

            var serial = new SerialNumber
            {
                SerialNumberValue = serialValue,
                ProductId = productId,
                LocationId = locationId,
                BatchId = batchId,
                Status = SerialStatus.Available,
                ReceivedDate = DateTime.UtcNow,
                Cost = 0m // TODO: Calculate from batch or product
            };

            var result = await _serialNumberRepo.AddAsync(serial, cancellationToken);
            await _trackingRepo.AddAsync(new SerialTracking
            {
                SerialNumberId = result.Id,
                MovementType = SerialMovementType.Received,
                ToLocationId = locationId,
                UserId = 1, // TODO: Get from context
                Notes = $"Serial {serialValue} generated for product {productId}"
            }, cancellationToken);
            await _auditService.LogAsync(nameof(SerialNumber), result.Id, "Generate", null, result, 1, "System", $"Generated serial {serialValue} for product {productId}", cancellationToken);
            return result;
        }

        public async Task<SerialNumber?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
        {
            return await _serialNumberRepo.GetBySerialNumberAsync(serialNumber, cancellationToken);
        }

        public async Task<IReadOnlyList<SerialNumber>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _serialNumberRepo.GetByProductIdAsync(productId, cancellationToken);
        }

        public async Task<IReadOnlyList<SerialNumber>> GetAvailableByProductAsync(int productId, int? locationId, CancellationToken cancellationToken = default)
        {
            return await _serialNumberRepo.GetAvailableByProductIdAsync(productId, locationId, cancellationToken);
        }

        public async Task<SerialNumber> AssignToSaleAsync(string serialNumber, int saleId, int saleDetailId, int userId, CancellationToken cancellationToken = default)
        {
            var serial = await _serialNumberRepo.GetBySerialNumberAsync(serialNumber, cancellationToken);
            if (serial == null)
                throw new InvalidOperationException($"Serial number {serialNumber} not found.");

            if (serial.Status != SerialStatus.Available)
                throw new InvalidOperationException($"Serial number {serialNumber} is not available for sale (current status: {serial.Status}).");

            serial.Status = SerialStatus.Sold;
            await _serialNumberRepo.UpdateAsync(serial, cancellationToken);

            await _trackingRepo.AddAsync(new SerialTracking
            {
                SerialNumberId = serial.Id,
                MovementType = SerialMovementType.Sold,
                FromLocationId = serial.LocationId,
                ReferenceId = saleId,
                ReferenceType = "Sale",
                UserId = userId,
                Notes = $"Serial {serialNumber} sold on sale {saleId}"
            }, cancellationToken);
            await _auditService.LogAsync(nameof(SerialNumber), serial.Id, "AssignToSale", null, serial, userId, "System", $"Assigned serial {serialNumber} to sale {saleId}", cancellationToken);
            return serial;
        }

        public async Task<SerialNumber> ReturnAsync(string serialNumber, int returnId, int userId, string? reason, CancellationToken cancellationToken = default)
        {
            var serial = await _serialNumberRepo.GetBySerialNumberAsync(serialNumber, cancellationToken);
            if (serial == null)
                throw new InvalidOperationException($"Serial number {serialNumber} not found.");

            if (serial.Status != SerialStatus.Sold && serial.Status != SerialStatus.Returned)
                throw new InvalidOperationException($"Serial number {serialNumber} is not sold (current status: {serial.Status}).");

            serial.Status = SerialStatus.Returned;
            await _serialNumberRepo.UpdateAsync(serial, cancellationToken);

            await _trackingRepo.AddAsync(new SerialTracking
            {
                SerialNumberId = serial.Id,
                MovementType = SerialMovementType.Returned,
                ToLocationId = serial.LocationId, // Returned to same location
                ReferenceId = returnId,
                ReferenceType = "SaleReturn",
                UserId = userId,
                Notes = $"Serial {serialNumber} returned for {returnId}: {reason ?? "No reason"}"
            }, cancellationToken);
            await _auditService.LogAsync(nameof(SerialNumber), serial.Id, "Return", null, serial, userId, "System", $"Returned serial {serialNumber} for return {returnId}", cancellationToken);
            return serial;
        }

        public async Task<SerialNumber> TransferAsync(string serialNumber, int fromLocationId, int toLocationId, int transferId, int userId, CancellationToken cancellationToken = default)
        {
            var serial = await _serialNumberRepo.GetBySerialNumberAsync(serialNumber, cancellationToken);
            if (serial == null)
                throw new InvalidOperationException($"Serial number {serialNumber} not found.");

            if (serial.LocationId != fromLocationId)
                throw new InvalidOperationException($"Serial number {serialNumber} is not at location {fromLocationId}.");

            if (serial.Status != SerialStatus.Available && serial.Status != SerialStatus.InTransit)
                throw new InvalidOperationException($"Serial number {serialNumber} is not available for transfer (current status: {serial.Status}).");

            serial.Status = SerialStatus.InTransit;
            await _serialNumberRepo.UpdateAsync(serial, cancellationToken);

            await _trackingRepo.AddAsync(new SerialTracking
            {
                SerialNumberId = serial.Id,
                MovementType = SerialMovementType.Transferred,
                FromLocationId = fromLocationId,
                ToLocationId = toLocationId,
                ReferenceId = transferId,
                ReferenceType = "StockTransfer",
                UserId = userId,
                Notes = $"Serial {serialNumber} transferred from {fromLocationId} to {toLocationId}"
            }, cancellationToken);
            await _auditService.LogAsync(nameof(SerialNumber), serial.Id, "Transfer", null, serial, userId, "System", $"Transferred serial {serialNumber} from {fromLocationId} to {toLocationId}", cancellationToken);
            return serial;
        }

        public async Task<SerialNumber> MarkExpiredAsync(string serialNumber, int userId, CancellationToken cancellationToken = default)
        {
            var serial = await _serialNumberRepo.GetBySerialNumberAsync(serialNumber, cancellationToken);
            if (serial == null)
                throw new InvalidOperationException($"Serial number {serialNumber} not found.");

            serial.Status = SerialStatus.Expired;
            await _serialNumberRepo.UpdateAsync(serial, cancellationToken);

            await _trackingRepo.AddAsync(new SerialTracking
            {
                SerialNumberId = serial.Id,
                MovementType = SerialMovementType.Expired,
                FromLocationId = serial.LocationId,
                UserId = userId,
                Notes = $"Serial {serialNumber} marked as expired"
            }, cancellationToken);
            await _auditService.LogAsync(nameof(SerialNumber), serial.Id, "MarkExpired", null, serial, userId, "System", $"Marked serial {serialNumber} as expired", cancellationToken);
            return serial;
        }

        public async Task<SerialNumber> MarkDamagedAsync(string serialNumber, string reason, int userId, CancellationToken cancellationToken = default)
        {
            var serial = await _serialNumberRepo.GetBySerialNumberAsync(serialNumber, cancellationToken);
            if (serial == null)
                throw new InvalidOperationException($"Serial number {serialNumber} not found.");

            serial.Status = SerialStatus.Damaged;
            await _serialNumberRepo.UpdateAsync(serial, cancellationToken);

            await _trackingRepo.AddAsync(new SerialTracking
            {
                SerialNumberId = serial.Id,
                MovementType = SerialMovementType.Damaged,
                FromLocationId = serial.LocationId,
                UserId = userId,
                Notes = $"Serial {serialNumber} marked as damaged: {reason}"
            }, cancellationToken);
            await _auditService.LogAsync(nameof(SerialNumber), serial.Id, "MarkDamaged", null, serial, userId, "System", $"Marked serial {serialNumber} as damaged: {reason}", cancellationToken);
            return serial;
        }

        public async Task<bool> ValidateSerialAsync(string serialNumber, int productId, CancellationToken cancellationToken = default)
        {
            var serial = await _serialNumberRepo.GetBySerialNumberAsync(serialNumber, cancellationToken);
            if (serial == null) return false;
            if (serial.ProductId != productId) return false;
            if (!serial.IsActive) return false;
            return serial.Status == SerialStatus.Available || serial.Status == SerialStatus.InTransit;
        }

        public async Task<string> GenerateSerialNumberAsync(int productId, CancellationToken cancellationToken = default)
        {
            var prefix = $"SN-{productId:D4}-{DateTime.UtcNow:yyyy}";
            var existing = await _serialNumberRepo.GetByProductIdAsync(productId, cancellationToken);
            var lastNumber = existing
                .Where(s => s.SerialNumberValue.StartsWith(prefix))
                .Select(s => int.TryParse(s.SerialNumberValue.Split('-').Last(), out var num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max();
            var nextNum = lastNumber + 1;
            return $"{prefix}-{nextNum:D6}";
        }

        public async Task<IReadOnlyList<SerialNumber>> GetAllSerialsAsync(int? locationId = null, CancellationToken cancellationToken = default)
        {
            var all = await _serialNumberRepo.GetAllAsync(cancellationToken);
            var result = all.Where(s => s.IsActive).ToList();
            if (locationId.HasValue)
                result = result.Where(s => s.LocationId == locationId.Value).ToList();
            return result;
        }
    }
}
