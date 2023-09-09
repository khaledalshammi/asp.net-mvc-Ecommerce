using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class CommentImagesRepository : Repository<CommentImages>, ICommentImagesRepository
    {
        private ApplicationDbContext _db;
        public CommentImagesRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(CommentImages obj)
        {
            _db.CommentImages.Update(obj);
        }
    }
}
