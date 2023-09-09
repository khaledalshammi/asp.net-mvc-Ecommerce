using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class JobRepository : Repository<Job>, IJobRepository
    {
        private ApplicationDbContext _db;
        public JobRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Job obj)
        {
            _db.Jobs.Update(obj);
        }
    }
}
