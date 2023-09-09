using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class NewsRepository : Repository<News>, INewsRepository
    {
        private ApplicationDbContext _db;
        public NewsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(News obj)
        {
            _db.News.Update(obj);
        }
    }
}
