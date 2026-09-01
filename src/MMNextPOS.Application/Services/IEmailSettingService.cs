using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;

namespace MMNextPOS.Application.Services
{
    public interface IEmailSettingService
    {
        Task<EmailSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<EmailSetting>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<EmailSetting> AddAsync(EmailSetting emailSetting, CancellationToken cancellationToken = default);
        Task UpdateAsync(EmailSetting emailSetting, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}