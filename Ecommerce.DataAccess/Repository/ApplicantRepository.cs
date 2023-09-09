using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class ApplicantRepository : Repository<Applicant>, IApplicantRepository
    {
        private ApplicationDbContext _db;
        public ApplicantRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Applicant obj)
        {
            _db.Applicants.Update(obj);
        }
    }
}
