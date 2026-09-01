using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public class EmailSettingRepository : GenericRepository<EmailSetting>, IEmailSettingRepository
    {
        public EmailSettingRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork, "EmailSettings")
        {
        }
    }
}