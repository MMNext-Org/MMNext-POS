using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using MMNextPOS.Application;

namespace MMNextPOS.Application.Services
{
    public class ExpiryManagementService
    {
        private readonly ISerialBatchRepository _batchRepo;
        private readonly ISerialNumberRepository _serialRepo;
        private readonly IProductRepository _productRepo;
        private readonly IAuditService _auditService;

        public ExpiryManagementService(
            ISerialBatchRepository batchRepo,
            ISerialNumberRepository serialRepo,
            IProductRepository productRepo,
            IAuditService auditService)
        {
            _batchRepo = batchRepo ?? throw new ArgumentNullException(nameof(batchRepo));
            _serialRepo = serialRepo ?? throw new ArgumentNullException(nameof(serialRepo));
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Gets all batches expiring soon (within the specified days).
        /// </summary>
        public async Task<IReadOnlyList<SerialBatch>> GetExpiringBatchesAsync(int daysThreshold = 30, CancellationToken cancellationToken = default)
        {
            return await _batchRepo.GetExpiringBatchesAsync(daysThreshold, cancellationToken);
        }

        /// <summary>
        /// Gets all expired batches.
        /// </summary>
        public async Task<IReadOnlyList<SerialBatch>> GetExpiredBatchesAsync(CancellationToken cancellationToken = default)
        {
            return await _batchRepo.GetExpiredBatchesAsync(cancellationToken);
        }

        /// <summary>
        /// Marks a batch as expired and creates audit record.
        /// </summary>
        public async Task<SerialBatch> MarkBatchExpiredAsync(int batchId, int expiredByUserId, CancellationToken cancellationToken = default)
        {
            var batch = await _batchRepo.GetByIdAsync(batchId, cancellationToken);
            if (batch == null)
                throw new KeyNotFoundException($"Batch {batchId} not found");

            // Deactivate the batch
            batch.IsActive = false;
            await _batchRepo.UpdateAsync(batch, cancellationToken);

            await _auditService.LogAsync(nameof(SerialBatch), batchId, "MarkExpired", null, batch, expiredByUserId, "System",
                $"Batch {batch.BatchNumber} marked as expired (Product: {batch.ProductId})", cancellationToken);

            return batch;
        }

        /// <summary>
        /// Processes expiring batches and creates stock movements for expired items.
        /// </summary>
        public async Task ProcessExpiryBatchAsync(CancellationToken cancellationToken = default)
        {
            var expiredBatches = await GetExpiredBatchesAsync(cancellationToken);
            if (!expiredBatches.Any())
                return;

            foreach (var batch in expiredBatches)
            {
                // Update serial number status to expired
                var serials = await _serialRepo.GetByProductIdAsync(batch.ProductId, cancellationToken);
                var batchSerials = serials.Where(s => s.BatchId == batch.Id && s.Status == SerialStatus.Available).ToList();

                foreach (var serial in batchSerials)
                {
                    serial.Status = SerialStatus.Expired;
                    await _serialRepo.UpdateAsync(serial, cancellationToken);

                    await _auditService.LogAsync(nameof(SerialNumber), serial.Id, "MarkExpired", null, serial, 1, "System",
                        $"Serial {serial.SerialNumberValue} marked as expired (Batch: {batch.BatchNumber})", cancellationToken);
                }

                // Update batch status
                batch.IsActive = false;
                await _batchRepo.UpdateAsync(batch, cancellationToken);
            }
        }

        /// <summary>
        /// Gets FEFO (First Expired, First Out) batches for a product.
        /// </summary>
        public async Task<IReadOnlyList<SerialBatch>> GetFefoBatchesAsync(int productId, int locationId, CancellationToken cancellationToken = default)
        {
            var batches = await _batchRepo.GetActiveBatchesByProductAsync(productId, cancellationToken);
            return batches.OrderBy(b => b.ExpiryDate).ThenBy(b => b.ManufactureDate).ToList();
        }
    }
}
