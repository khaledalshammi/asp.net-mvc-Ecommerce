using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        private ApplicationDbContext _db;
        public SubscriptionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Subscription obj)
        {
            _db.Subscriptions.Update(obj);
        }
    }
}
