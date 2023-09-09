using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        private ApplicationDbContext _db;
        public ImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Image obj)
        {
            _db.Images.Update(obj);
        }
    }
}
