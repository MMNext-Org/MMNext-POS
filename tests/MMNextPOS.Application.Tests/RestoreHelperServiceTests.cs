using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    /// <summary>
    /// Unit tests for RestoreHelperService (static validation helpers).
    /// </summary>
    public class RestoreHelperServiceTests
    {
        [Fact]
        public async Task ValidateBackupFileAsync_ValidFile_ReturnsValid()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempFile, new byte[] { 0x50, 0x4B, 0x03, 0x04 });

            // Act
            var result = await MMNextPOS.Application.Services.RestoreHelperService.ValidateBackupFileAsync(tempFile);

            // Assert
            Assert.True(result.IsValid);
            Assert.True(result.FileSizeBytes >= 4);
            Assert.Equal(tempFile, result.FilePath);

            // Cleanup
            File.Delete(tempFile);
        }

        [Fact]
        public async Task ValidateBackupFileAsync_NonExistentFile_ReturnsInvalid()
        {
            // Act
            var result = await MMNextPOS.Application.Services.RestoreHelperService.ValidateBackupFileAsync("C:\\nonexistent\\backup.zip");

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Backup file does not exist: C:\\nonexistent\\backup.zip", result.ErrorMessage);
        }
    }
}
