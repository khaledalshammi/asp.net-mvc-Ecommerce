using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class SubscriptionTypeRepository : Repository<SubscriptionType>, ISubscriptionTypeRepository
    {
        private ApplicationDbContext _db;
        public SubscriptionTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(SubscriptionType obj)
        {
            _db.SubscriptionTypes.Update(obj);
        }
    }
}
