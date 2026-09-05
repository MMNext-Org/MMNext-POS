using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class LicenseInfoService : ILicenseInfoService
    {
        private readonly ILicenseInfoRepository _repo;
        private readonly IDeviceInfoRepository _deviceRepo;
        private readonly IDeviceFingerprintService _fingerprintService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public LicenseInfoService(
            ILicenseInfoRepository repo,
            IDeviceInfoRepository deviceRepo,
            IDeviceFingerprintService fingerprintService,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _deviceRepo = deviceRepo ?? throw new ArgumentNullException(nameof(deviceRepo));
            _fingerprintService = fingerprintService ?? throw new ArgumentNullException(nameof(fingerprintService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public Task<LicenseInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repo.GetByIdAsync(id, cancellationToken);
        }

        public async Task<LicenseInfo?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default)
        {
            var all = await _repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return all.FirstOrDefault(l => l.LicenseKey == licenseKey);
        }

        public Task<LicenseInfo?> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetCurrentAsync(cancellationToken);
        }

        public Task<IReadOnlyList<LicenseInfo>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _repo.GetAllAsync(cancellationToken);
        }

        public async Task<LicenseInfo> AddAsync(LicenseInfo entity, CancellationToken cancellationToken = default)
        {
            var result = await _repo.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(LicenseInfo), result.Id, "Create", null, result, 1, "System", $"Created license {result.LicenseKey}", cancellationToken).ConfigureAwait(false);
            return result;
        }

        public async Task UpdateAsync(LicenseInfo entity, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(entity.Id, cancellationToken).ConfigureAwait(false);
            await _repo.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(LicenseInfo), entity.Id, "Update", existing, entity, 1, "System", $"Updated license {entity.LicenseKey}", cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            await _repo.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            await _auditService.LogAsync(nameof(LicenseInfo), id, "Delete", existing, null, 1, "System", $"Deleted license {existing?.LicenseKey ?? id.ToString()}", cancellationToken).ConfigureAwait(false);
        }

        public async Task<LicenseInfo> ActivateAsync(LicenseActivationRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateRequest(request);

            var fingerprint = _fingerprintService.GetCurrent();

            // Confirm there is still room on the license for this device.
            var existing = await _repo.GetByLicenseKeyAsync(request.LicenseKey, cancellationToken).ConfigureAwait(false);
            var alreadyBound = await _deviceRepo.GetByFingerprintAsync(fingerprint.Hash, cancellationToken).ConfigureAwait(false);
            var activeDeviceCount = await _deviceRepo.CountActiveAsync(cancellationToken).ConfigureAwait(false);

            if (alreadyBound == null && activeDeviceCount >= request.MaxDevices)
            {
                throw new InvalidOperationException(
                    $"This license allows {request.MaxDevices} device(s) and that limit has been reached. " +
                    "Deactivate another device before activating a new one.");
            }

            var now = DateTime.UtcNow;
            var license = new LicenseInfo
            {
                LicenseKey = request.LicenseKey.Trim(),
                CompanyName = request.CompanyName.Trim(),
                ContactPerson = request.ContactPerson?.Trim() ?? string.Empty,
                Email = request.Email?.Trim() ?? string.Empty,
                Phone = request.Phone?.Trim() ?? string.Empty,
                Address = request.Address?.Trim() ?? string.Empty,
                RegistrationDate = now,
                ExpiryDate = now.AddDays(Math.Max(1, request.SubscriptionDays)),
                MaxUsers = Math.Max(1, request.MaxUsers),
                MaxDevices = Math.Max(1, request.MaxDevices),
                Status = "Active",
                IsActivated = true,
                ActivatedDate = now,
                ActivatedDeviceId = fingerprint.Hash,
                Notes = $"Activated on {Environment.MachineName} ({fingerprint.MacAddress})"
            };

            await _unitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                license = await _repo.AddAsync(license, cancellationToken).ConfigureAwait(false);

                var device = alreadyBound ?? new DeviceInfo
                {
                    RegistrationId = license.Id,
                    DeviceFingerprint = fingerprint.Hash,
                    DeviceName = Environment.MachineName,
                    MacAddress = fingerprint.MacAddress,
                    CpuId = fingerprint.CpuId,
                    HardDiskSerial = fingerprint.HardDiskSerial,
                    OsVersion = fingerprint.OsVersion,
                    IsActive = true,
                    LastSeenAt = now
                };

                if (alreadyBound == null)
                {
                    await _deviceRepo.AddAsync(device, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Re-activation on the same device: refresh fields so the
                    // new license's footprint is reflected.
                    alreadyBound.RegistrationId = license.Id;
                    alreadyBound.IsActive = true;
                    alreadyBound.LastSeenAt = now;
                    await _deviceRepo.UpdateAsync(alreadyBound, cancellationToken).ConfigureAwait(false);
                }

                await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }

            await _auditService.LogAsync(
                nameof(LicenseInfo),
                license.Id,
                "Activate",
                existing,
                license,
                userId: 1,
                userName: "System",
                description: $"Activated license {license.LicenseKey} on device {fingerprint.Hash}",
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return license;
        }

        private static void ValidateRequest(LicenseActivationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LicenseKey))
                throw new ArgumentException("License key is required.", nameof(request));
            if (request.LicenseKey.Trim().Length < 8)
                throw new ArgumentException("License key is too short.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.CompanyName))
                throw new ArgumentException("Company name is required.", nameof(request));
            if (request.SubscriptionDays <= 0)
                throw new ArgumentException("Subscription days must be greater than zero.", nameof(request));
            if (request.MaxDevices <= 0)
                throw new ArgumentException("Max devices must be greater than zero.", nameof(request));
            if (request.MaxUsers <= 0)
                throw new ArgumentException("Max users must be greater than zero.", nameof(request));
        }
    }
}
