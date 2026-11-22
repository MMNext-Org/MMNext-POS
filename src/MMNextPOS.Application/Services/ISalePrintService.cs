using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface ISalePrintService
    {
        Task<string> GenerateSalePrintAsync(Sale sale);
    }
}