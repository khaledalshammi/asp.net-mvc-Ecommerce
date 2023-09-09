using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository.IRepository
{
    public interface ICommentRepository : IRepository<Comment>
    {
        void Update(Comment obj);
    }
}
