using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class LikeRepository : Repository<Like>, ILikeRepository
    {
        private ApplicationDbContext _db;
        public LikeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Like obj)
        {
            _db.Likes.Update(obj);
        }
    }
}
