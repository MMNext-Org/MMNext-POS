using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Carries the data captured by <see cref="LicenseRegistrationForm"/>
    /// to <see cref="ILicenseInfoService.ActivateAsync"/>. A single record
    /// means: register a new license, bind this device, and start the clock.
    /// </summary>
    public sealed record LicenseActivationRequest(
        string LicenseKey,
        string CompanyName,
        string? ContactPerson,
        string? Email,
        string? Phone,
        string? Address,
        int MaxUsers,
        int MaxDevices,
        int SubscriptionDays);

    public interface ILicenseInfoService
    {
        Task<LicenseInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<LicenseInfo?> GetByLicenseKeyAsync(string licenseKey, CancellationToken cancellationToken = default);
        Task<LicenseInfo?> GetCurrentAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LicenseInfo>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<LicenseInfo> AddAsync(LicenseInfo entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(LicenseInfo entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the request, writes a new <see cref="LicenseInfo"/> row,
        /// and binds the current device in a single transaction. Returns the
        /// persisted license. Throws <see cref="ArgumentException"/> for
        /// invalid input and <see cref="InvalidOperationException"/> when the
        /// device limit would be exceeded.
        /// </summary>
        Task<LicenseInfo> ActivateAsync(LicenseActivationRequest request, CancellationToken cancellationToken = default);
    }
}
