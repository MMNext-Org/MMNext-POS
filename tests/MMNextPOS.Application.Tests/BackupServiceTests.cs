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
    /// <summary>
    /// Unit tests for BackupService.
    /// </summary>
    public class BackupServiceTests
    {
        private readonly Mock<IBackupSettingRepository> _repoMock = new();
        private readonly Mock<IAuditService> _auditServiceMock = new();

        private IBackupService CreateService()
        {
            return new BackupService(_repoMock.Object, _auditServiceMock.Object);
        }

        private void SetupHappyPath()
        {
            var backupSetting = new BackupSetting
            {
                Id = 1,
                Name = "DailyBackup",
                IsActive = true,
                Frequency = "Daily",
                RetentionDays = 7,
                IncludeFiles = false,
                BackupPath = "C:\\backups\\"
            };

            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(backupSetting);

            _repoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(backupSetting);

            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { backupSetting });

            _repoMock.Setup(r => r.AddAsync(It.IsAny<BackupSetting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BackupSetting bs, CancellationToken _) => { bs.Id = 1; return bs; });

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<BackupSetting>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingBackupSetting_ReturnsBackupSetting()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("DailyBackup", result!.Name);
        }

        [Fact]
        public async Task GetByNameAsync_ExistingName_ReturnsBackupSetting()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetByNameAsync("DailyBackup");

            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllBackupSettings()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task AddAsync_ValidBackupSetting_CreatesAndAuditLogs()
        {
            SetupHappyPath();
            var service = CreateService();

            var newSetting = new BackupSetting
            {
                Name = "HourlyBackup",
                IsActive = true,
                Frequency = "Daily",
                RetentionDays = 30,
                BackupPath = "C:\\backups\\hourly"
            };

            var result = await service.AddAsync(newSetting);

            Assert.Equal(1, result.Id);
            _repoMock.Verify(r => r.AddAsync(newSetting, It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(BackupSetting), 1, "Create", null, newSetting, 1, "System", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingBackupSetting_UpdatesAndAudits()
        {
            SetupHappyPath();
            var service = CreateService();

            var updatedSetting = new BackupSetting
            {
                Id = 1,
                Name = "DailyBackup-Updated",
                IsActive = true,
                Frequency = "Weekly",
                RetentionDays = 14
            };

            await service.UpdateAsync(updatedSetting);

            _repoMock.Verify(r => r.UpdateAsync(updatedSetting, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingBackupSetting_DeletesAndAudits()
        {
            SetupHappyPath();
            var service = CreateService();

            await service.DeleteAsync(1);

            _repoMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RunBackupAsync_ValidBackupSetting_ReturnsTrue()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.RunBackupAsync(1);

            Assert.True(result);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<BackupSetting>(), It.IsAny<CancellationToken>()), Times.Once);
            _auditServiceMock.Verify(a => a.LogAsync(nameof(BackupSetting), 1, "Backup", It.IsAny<object>(), It.IsAny<object>(), 1, "System", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RunBackupAsync_MissingBackupSetting_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BackupSetting?)null);
            var service = CreateService();

            var result = await service.RunBackupAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task RestoreAsync_ValidBackupSetting_ReturnsTrue()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.RestoreAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task RestoreAsync_MissingBackupSetting_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BackupSetting?)null);
            var service = CreateService();

            var result = await service.RestoreAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task GetBackupHistoryAsync_ValidSetting_ReturnsEmptyList()
        {
            SetupHappyPath();
            var service = CreateService();

            var result = await service.GetBackupHistoryAsync(1);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
