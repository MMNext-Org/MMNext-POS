using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    /// <summary>
    /// Tests for MyanmarFontVerificationService.
    /// </summary>
    public class MyanmarFontVerificationServiceTests
    {
        [Fact]
        public void VerifyFonts_ReturnsValidResult()
        {
            // Arrange
            var service = new MMNextPOS.Application.Services.MyanmarFontVerificationService(new MockAuditService());

            // Act
            var result = service.VerifyFonts();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void VerifyFonts_ReturnsFontFamilyName()
        {
            // Arrange
            var service = new MMNextPOS.Application.Services.MyanmarFontVerificationService(new MockAuditService());

            // Act
            var result = service.VerifyFonts();

            // Assert
            Assert.NotNull(result.FontFamilyName);
            Assert.Equal("Arial Unicode MS", result.FontFamilyName);
        }
    }

    public class MockAuditService : MMNextPOS.Application.Services.IAuditService
    {
        public Task LogAsync(string entityName, int entityId, string action, object? oldValues, object? newValues, int? userId = null, string? userName = null, string? description = null, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
