using System;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure;
using MMNextPOS.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class LicenseInfoServiceActivateTests
    {
        private readonly Mock<ILicenseInfoRepository> _licenseRepoMock = new();
        private readonly Mock<IDeviceInfoRepository> _deviceRepoMock = new();
        private readonly Mock<IDeviceFingerprintService> _fingerprintMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<IAuditService> _auditMock = new();

        private static readonly DeviceFingerprint SampleFingerprint = new(
            Hash: "abc123def456",
            MachineName: "POS-01",
            MacAddress: "00:11:22:33:44:55",
            CpuId: null,
            HardDiskSerial: null,
            OsVersion: "Microsoft Windows 10.0.26200");

        public LicenseInfoServiceActivateTests()
        {
            _fingerprintMock.Setup(f => f.GetCurrent()).Returns(SampleFingerprint);
        }

        private LicenseInfoService CreateService() => new(
            _licenseRepoMock.Object,
            _deviceRepoMock.Object,
            _fingerprintMock.Object,
            _uowMock.Object,
            _auditMock.Object);

        private static LicenseActivationRequest ValidRequest() => new(
            LicenseKey: "MMNEXT-ABCD-1234-5678",
            CompanyName: "Acme Co",
            ContactPerson: "Jane Doe",
            Email: "ops@acme.test",
            Phone: "+95 9 123 456 789",
            Address: "Yangon",
            MaxUsers: 5,
            MaxDevices: 2,
            SubscriptionDays: 365);

        [Fact]
        public async Task ActivateAsync_ValidRequest_PersistsLicenseAndDevice()
        {
            _licenseRepoMock.Setup(r => r.AddAsync(It.IsAny<LicenseInfo>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((LicenseInfo l, CancellationToken _) => { l.Id = 42; return l; });
            _deviceRepoMock.Setup(r => r.GetByFingerprintAsync(SampleFingerprint.Hash, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((DeviceInfo?)null);
            _deviceRepoMock.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            _licenseRepoMock.Setup(r => r.GetByLicenseKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((LicenseInfo?)null);

            var service = CreateService();
            var result = await service.ActivateAsync(ValidRequest());

            Assert.Equal(42, result.Id);
            Assert.Equal("MMNEXT-ABCD-1234-5678", result.LicenseKey);
            Assert.True(result.IsActivated);
            Assert.Equal(SampleFingerprint.Hash, result.ActivatedDeviceId);
            Assert.True(result.ExpiryDate > DateTime.UtcNow);

            _deviceRepoMock.Verify(r => r.AddAsync(It.IsAny<DeviceInfo>(), It.IsAny<CancellationToken>()), Times.Once);
            _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _auditMock.Verify(a => a.LogAsync(
                nameof(LicenseInfo), 42, "Activate",
                It.IsAny<LicenseInfo?>(), It.IsAny<LicenseInfo>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ActivateAsync_DeviceAlreadyBound_UpdatesRatherThanReinsert()
        {
            var existingDevice = new DeviceInfo
            {
                Id = 7,
                DeviceFingerprint = SampleFingerprint.Hash,
                IsActive = true
            };
            _licenseRepoMock.Setup(r => r.AddAsync(It.IsAny<LicenseInfo>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((LicenseInfo l, CancellationToken _) => { l.Id = 99; return l; });
            _licenseRepoMock.Setup(r => r.GetByLicenseKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((LicenseInfo?)null);
            _deviceRepoMock.Setup(r => r.GetByFingerprintAsync(SampleFingerprint.Hash, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existingDevice);
            _deviceRepoMock.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = CreateService();
            var result = await service.ActivateAsync(ValidRequest());

            Assert.Equal(99, result.Id);
            _deviceRepoMock.Verify(r => r.AddAsync(It.IsAny<DeviceInfo>(), It.IsAny<CancellationToken>()), Times.Never);
            _deviceRepoMock.Verify(r => r.UpdateAsync(existingDevice, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ActivateAsync_DeviceLimitReached_Throws()
        {
            _licenseRepoMock.Setup(r => r.GetByLicenseKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((LicenseInfo?)null);
            _deviceRepoMock.Setup(r => r.GetByFingerprintAsync(SampleFingerprint.Hash, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((DeviceInfo?)null);
            _deviceRepoMock.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

            var request = ValidRequest() with { MaxDevices = 2 };
            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ActivateAsync(request));
        }

        [Fact]
        public async Task ActivateAsync_EmptyKey_ThrowsArgument()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ActivateAsync(ValidRequest() with { LicenseKey = "  " }));
        }

        [Fact]
        public async Task ActivateAsync_NoCompany_ThrowsArgument()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ActivateAsync(ValidRequest() with { CompanyName = "" }));
        }

        [Fact]
        public async Task ActivateAsync_ZeroDays_ThrowsArgument()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ActivateAsync(ValidRequest() with { SubscriptionDays = 0 }));
        }
    }
}
