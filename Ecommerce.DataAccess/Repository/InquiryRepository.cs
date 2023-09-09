using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class InquiryRepository : Repository<Inquiry>, IInquiryRepository
    {
        private ApplicationDbContext _db;
        public InquiryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Inquiry obj)
        {
            _db.Inquiries.Update(obj);
        }
    }
}
