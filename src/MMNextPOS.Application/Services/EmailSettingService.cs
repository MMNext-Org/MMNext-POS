using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MMNextPOS.Domain.Models;
using MMNextPOS.Infrastructure.Repositories;

namespace MMNextPOS.Application.Services
{
    public class EmailSettingService : IEmailSettingService
    {
        private readonly IEmailSettingRepository _repo;

        public EmailSettingService(IEmailSettingRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<EmailSetting> AddAsync(EmailSetting emailSetting, CancellationToken cancellationToken = default)
            => _repo.AddAsync(emailSetting, cancellationToken);

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
            => _repo.DeleteAsync(id, cancellationToken);

        public Task<IReadOnlyList<EmailSetting>> GetAllAsync(CancellationToken cancellationToken = default)
            => _repo.GetAllAsync(cancellationToken);

        public Task<EmailSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _repo.GetByIdAsync(id, cancellationToken);

        public Task UpdateAsync(EmailSetting emailSetting, CancellationToken cancellationToken = default)
            => _repo.UpdateAsync(emailSetting, cancellationToken);
    }
}
