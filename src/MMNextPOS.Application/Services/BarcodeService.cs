using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public class BarcodeService : IBarcodeService
    {
        public string GenerateCode128(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Start Code B = 104
            var checksum = 104;
            var result = new StringBuilder();
            result.Append((char)104); // Start Code B

            int i = 0;
            while (i < input.Length)
            {
                int charCode = input[i];
                if (charCode < 32 || charCode > 126)
                    charCode = 32; // Replace invalid with space

                int charValue = charCode - 32;
                checksum += charValue * (i + 1);

                result.Append((char)(charValue + 100));
                i++;
            }

            result.Append((char)(checksum % 103 + 100)); // Checksum
            result.Append((char)233); // Stop
            result.Append((char)196);  // Stop pattern

            return result.ToString();
        }

        public string GenerateEan13(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length != 12)
                throw new ArgumentException("EAN-13 requires exactly 12 digits", nameof(input));

            if (!input.All(char.IsDigit))
                throw new ArgumentException("EAN-13 can only contain digits", nameof(input));

            // Calculate check digit
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = input[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            int checkDigit = (10 - (sum % 10)) % 10;

            return input + checkDigit;
        }

        public string GenerateQrData(string input)
        {
            // For QR codes, we just return the input (QR generation would use a library like QRCoder)
            return $"qr:{input}";
        }

        public bool ValidateCode128(string barcode)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Length < 3)
                return false;

            // Check start/stop characters - matches what GenerateCode128 produces
            // Start Code B = 104 (0x68), Stop = 233 (0xE9)
            char startChar = barcode[0];
            char stopChar = barcode[barcode.Length - 2];

            return startChar == (char)104 && stopChar == (char)233;
        }

        public bool ValidateEan13(string barcode)
        {
            if (string.IsNullOrEmpty(barcode) || barcode.Length != 13)
                return false;

            if (!barcode.All(char.IsDigit))
                return false;

            // Validate check digit
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = barcode[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            int checkDigit = (10 - (sum % 10)) % 10;
            return barcode[12] == (char)('0' + checkDigit);
        }

        public (string type, string value) ParseScannerInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return ("unknown", string.Empty);

            // Common scanner prefixes
            if (input.StartsWith("]C1")) // Code 128
                return ("code128", input.Substring(3));
            if (input.StartsWith("]E0")) // EAN-13
                return ("ean13", input.Substring(3));
            if (input.StartsWith("]Q1")) // QR Code
                return ("qr", input.Substring(3));
            if (input.StartsWith("SN-") || input.StartsWith("BATCH-"))
                return ("serial", input);

            // Default to code128 if it looks like a barcode
            if (input.Length >= 3 && input.All(c => c >= ' ' && c <= '~'))
                return ("code128", input);

            return ("unknown", input);
        }

        public Task<byte[]> GenerateBarcodeImageAsync(string barcode, int width = 300, int height = 100)
        {
            // This would use a library like SkiaSharp or DevExpress to generate the image
            // For now, return a placeholder
            throw new NotImplementedException("Barcode image generation requires external library (SkiaSharp, DevExpress, etc.)");
        }
    }
}
