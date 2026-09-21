using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IBarcodeService
    {
        string GenerateCode128(string input);
        string GenerateEan13(string input);
        string GenerateQrData(string input);
        bool ValidateCode128(string barcode);
        bool ValidateEan13(string barcode);
        (string type, string value) ParseScannerInput(string input);
        Task<byte[]> GenerateBarcodeImageAsync(string barcode, int width = 300, int height = 100);
    }
}
