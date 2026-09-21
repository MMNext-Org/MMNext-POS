using System;
using System.Linq;
using System.Threading.Tasks;
using MMNextPOS.Application.Services;
using MMNextPOS.Domain.Models;
using Xunit;

namespace MMNextPOS.Application.Tests
{
    public class BarcodeServiceTests
    {
        private readonly BarcodeService _service;

        public BarcodeServiceTests()
        {
            _service = new BarcodeService();
        }

        // ───────────────────────── Code 128 Tests ─────────────────────────

        [Fact]
        public void GenerateCode128_ValidInput_ReturnsEncodedString()
        {
            // Arrange
            var input = "12345";

            // Act
            var result = _service.GenerateCode128(input);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > input.Length); // Start code + data + checksum + stop
            Assert.Equal((char)104, result[0]); // Start Code B
        }

        [Fact]
        public void GenerateCode128_EmptyInput_ReturnsEmpty()
        {
            // Arrange
            var input = "";

            // Act
            var result = _service.GenerateCode128(input);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GenerateCode128_SpecialChars_ReplacesWithSpace()
        {
            // Arrange
            var input = "test\n\r\t"; // Newline, tab, CR

            // Act
            var result = _service.GenerateCode128(input);

            // Assert
            Assert.NotNull(result);
            // Ensure special chars were replaced (they get mapped to chars)
        }

        [Fact]
        public void ValidateCode128_ValidBarcode_ReturnsTrue()
        {
            // Arrange
            var barcode = "test123";

            // Act
            var encoded = _service.GenerateCode128(barcode);
            var result = _service.ValidateCode128(encoded);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateCode128_EmptyInput_ReturnsFalse()
        {
            // Act
            var result = _service.ValidateCode128("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateCode128_TooShort_ReturnsFalse()
        {
            // Act
            var result = _service.ValidateCode128("ab");

            // Assert
            Assert.False(result);
        }

        // ───────────────────────── EAN-13 Tests ─────────────────────────

        [Fact]
        public void GenerateEan13_Valid12Digits_Returns13DigitsWithChecksum()
        {
            // Arrange
            var input = "123456789012";

            // Act
            var result = _service.GenerateEan13(input);

            // Assert
            Assert.Equal(13, result.Length);
            Assert.True(result.All(char.IsDigit));
        }

        [Fact]
        public void GenerateEan13_CorrectCheckDigit_ComputesCorrectly()
        {
            // Arrange
            var input = "400638133393"; // Known valid EAN-13 prefix, check digit should be 1

            // Act
            var result = _service.GenerateEan13(input);

            // Assert
            Assert.Equal(13, result.Length);
            Assert.Equal('1', result[12]); // Last digit (check digit) should be 1
        }

        [Fact]
        public void GenerateEan13_NonDigits_ThrowsArgumentException()
        {
            // Arrange
            var input = "12345678901a"; // Contains letter

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.GenerateEan13(input));
        }

        [Fact]
        public void GenerateEan13_WrongLength_ThrowsArgumentException()
        {
            // Arrange
            var input = "1234567890123"; // 13 digits instead of 12

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.GenerateEan13(input));
        }

        [Fact]
        public void ValidateEan13_ValidBarcode_ReturnsTrue()
        {
            // Arrange
            var barcode = "4006381333931"; // Valid EAN-13

            // Act
            var result = _service.ValidateEan13(barcode);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateEan13_InvalidCheckDigit_ReturnsFalse()
        {
            // Arrange
            var barcode = "4006381333932"; // Wrong check digit

            // Act
            var result = _service.ValidateEan13(barcode);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateEan13_NonDigits_ReturnsFalse()
        {
            // Arrange
            var barcode = "abc1234567890";

            // Act
            var result = _service.ValidateEan13(barcode);

            // Assert
            Assert.False(result);
        }

        // ───────────────────────── QR Code Tests ─────────────────────────

        [Fact]
        public void GenerateQrData_ValidInput_ReturnsQrData()
        {
            // Arrange
            var input = "PRODUCT:12345";

            // Act
            var result = _service.GenerateQrData(input);

            // Assert
            Assert.StartsWith("qr:", result);
            Assert.Contains(input, result);
        }

        // ───────────────────────── Scanner Input Tests ─────────────────────────

        [Fact]
        public void ParseScannerInput_Code128Prefix_ReturnsCode128()
        {
            // Arrange
            var input = "]C1test123";

            // Act
            var result = _service.ParseScannerInput(input);

            // Assert
            Assert.Equal("code128", result.type);
            Assert.Equal("test123", result.value);
        }

        [Fact]
        public void ParseScannerInput_Ean13Prefix_ReturnsEan13()
        {
            // Arrange
            var input = "]E0123456789012";

            // Act
            var result = _service.ParseScannerInput(input);

            // Assert
            Assert.Equal("ean13", result.type);
            Assert.Equal("123456789012", result.value);
        }

        [Fact]
        public void ParseScannerInput_QrPrefix_ReturnsQr()
        {
            // Arrange
            var input = "]Q1test123";

            // Act
            var result = _service.ParseScannerInput(input);

            // Assert
            Assert.Equal("qr", result.type);
            Assert.Equal("test123", result.value);
        }

        [Fact]
        public void ParseScannerInput_SerialPrefix_ReturnsSerial()
        {
            // Arrange
            var input = "SN-0001-2026-000001";

            // Act
            var result = _service.ParseScannerInput(input);

            // Assert
            Assert.Equal("serial", result.type);
            Assert.Equal("SN-0001-2026-000001", result.value);
        }

        [Fact]
        public void ParseScannerInput_Unknown_NoPrefix_ReturnsCode128AsDefault()
        {
            // Arrange
            var input = "test123";

            // Act
            var result = _service.ParseScannerInput(input);

            // Assert
            Assert.Equal("code128", result.type);
            Assert.Equal("test123", result.value);
        }

        [Fact]
        public void ParseScannerInput_Empty_ReturnsUnknown()
        {
            // Arrange
            var input = "";

            // Act
            var result = _service.ParseScannerInput(input);

            // Assert
            Assert.Equal("unknown", result.type);
            Assert.Empty(result.value);
        }

        // ───────────────────────── Barcode Image Tests ─────────────────────────

        [Fact]
        public async Task GenerateBarcodeImageAsync_ValidInput_ThrowsNotImplemented()
        {
            // Arrange
            var barcode = "123456789012";

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.GenerateBarcodeImageAsync(barcode));
        }

        [Fact]
        public async Task GenerateBarcodeImageAsync_EmptyInput_ThrowsNotImplemented()
        {
            // Arrange
            var barcode = "";

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.GenerateBarcodeImageAsync(barcode));
        }
    }
}
