using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class DisLikeRepository : Repository<DisLike>, IDisLikeRepository
    {
        private ApplicationDbContext _db;
        public DisLikeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(DisLike obj)
        {
            _db.DisLikes.Update(obj);
        }
    }
}
