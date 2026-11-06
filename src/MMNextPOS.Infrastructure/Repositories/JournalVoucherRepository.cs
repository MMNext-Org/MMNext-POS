using MMNextPOS.Domain.Models;

namespace MMNextPOS.Infrastructure.Repositories
{
    public interface IJournalVoucherRepository : IRepository<JournalVoucher>
    {
    }

    public class JournalVoucherRepository : GenericRepository<JournalVoucher>, IJournalVoucherRepository
    {
        public JournalVoucherRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "JournalVouchers")
        {
        }
    }

    public interface IJournalVoucherDetailRepository : IRepository<JournalVoucherDetail>
    {
    }

    public class JournalVoucherDetailRepository : GenericRepository<JournalVoucherDetail>, IJournalVoucherDetailRepository
    {
        public JournalVoucherDetailRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork, "JournalVoucherDetails")
        {
        }
    }
}
