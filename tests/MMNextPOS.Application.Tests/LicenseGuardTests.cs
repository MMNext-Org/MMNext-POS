using System;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class LicenseGuardTests
    {
        private readonly Mock<ILicenseInfoRepository> _licenseRepoMock = new();
        private readonly Mock<IDeviceInfoRepository> _deviceRepoMock = new();
        private readonly Mock<IDeviceFingerprintService> _fingerprintMock = new();
        private readonly Mock<IAuditService> _auditMock = new();

        private static readonly DeviceFingerprint SampleFingerprint = new(
            Hash: "abc123def456",
            MachineName: "POS-01",
            MacAddress: "00:11:22:33:44:55",
            CpuId: null,
            HardDiskSerial: null,
            OsVersion: "Microsoft Windows 10.0.26200");

        public LicenseGuardTests()
        {
            _fingerprintMock.Setup(f => f.GetCurrent()).Returns(SampleFingerprint);
        }

        private LicenseGuard CreateGuard() =>
            new(_licenseRepoMock.Object, _deviceRepoMock.Object, _fingerprintMock.Object, _auditMock.Object);

        private static LicenseInfo ActiveLicense(DateTime? expiry = null) => new()
        {
            Id = 1,
            LicenseKey = "TEST-KEY-1234",
            CompanyName = "Acme Co",
            Status = "Active",
            IsActivated = true,
            RegistrationDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = expiry ?? DateTime.UtcNow.AddDays(30),
            MaxUsers = 5,
            MaxDevices = 1
        };

        [Fact]
        public async Task CheckAsync_NoLicense_ReturnsNotRegistered()
        {
            _licenseRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync((LicenseInfo?)null);

            var guard = CreateGuard();
            var result = await guard.CheckAsync();

            Assert.False(result.IsValid);
            Assert.Equal(LicenseInvalidReason.NotRegistered, result.Reason);
            Assert.Equal(SampleFingerprint, result.Fingerprint);
        }

        [Fact]
        public async Task CheckAsync_SuspendedLicense_ReturnsSuspended()
        {
            var license = ActiveLicense();
            license.Status = "Suspended";
            _licenseRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(license);

            var guard = CreateGuard();
            var result = await guard.CheckAsync();

            Assert.False(result.IsValid);
            Assert.Equal(LicenseInvalidReason.Suspended, result.Reason);
        }

        [Fact]
        public async Task CheckAsync_CancelledLicense_ReturnsCancelled()
        {
            var license = ActiveLicense();
            license.Status = "Cancelled";
            _licenseRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(license);

            var guard = CreateGuard();
            var result = await guard.CheckAsync();

            Assert.False(result.IsValid);
            Assert.Equal(LicenseInvalidReason.Cancelled, result.Reason);
        }

        [Fact]
        public async Task CheckAsync_ExpiredLicense_ReturnsExpired()
        {
            var license = ActiveLicense(expiry: DateTime.UtcNow.AddDays(-1));
            _licenseRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(license);

            var guard = CreateGuard();
            var result = await guard.CheckAsync();

            Assert.False(result.IsValid);
            Assert.Equal(LicenseInvalidReason.Expired, result.Reason);
        }

        [Fact]
        public async Task CheckAsync_DeviceNotBound_ReturnsDeviceNotBound()
        {
            var license = ActiveLicense();
            _licenseRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(license);
            _deviceRepoMock.Setup(r => r.GetByFingerprintAsync(SampleFingerprint.Hash, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((DeviceInfo?)null);

            var guard = CreateGuard();
            var result = await guard.CheckAsync();

            Assert.False(result.IsValid);
            Assert.Equal(LicenseInvalidReason.DeviceNotBound, result.Reason);
            Assert.NotNull(result.License);
        }

        [Fact]
        public async Task CheckAsync_InactiveDevice_ReturnsDeviceNotBound()
        {
            var license = ActiveLicense();
            var device = new DeviceInfo { DeviceFingerprint = SampleFingerprint.Hash, IsActive = false };
            _licenseRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(license);
            _deviceRepoMock.Setup(r => r.GetByFingerprintAsync(SampleFingerprint.Hash, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(device);

            var guard = CreateGuard();
            var result = await guard.CheckAsync();

            Assert.False(result.IsValid);
            Assert.Equal(LicenseInvalidReason.DeviceNotBound, result.Reason);
        }

        [Fact]
        public async Task CheckAsync_ValidLicenseAndDevice_ReturnsValid()
        {
            var license = ActiveLicense();
            var device = new DeviceInfo { DeviceFingerprint = SampleFingerprint.Hash, IsActive = true };
            _licenseRepoMock.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(license);
            _deviceRepoMock.Setup(r => r.GetByFingerprintAsync(SampleFingerprint.Hash, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(device);

            var guard = CreateGuard();
            var result = await guard.CheckAsync();

            Assert.True(result.IsValid);
            Assert.Equal(LicenseInvalidReason.Valid, result.Reason);
            Assert.Same(license, result.License);
            Assert.Same(device, result.Device);
        }
    }
}
