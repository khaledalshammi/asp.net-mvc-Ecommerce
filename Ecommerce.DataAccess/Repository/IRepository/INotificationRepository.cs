using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository.IRepository
{
    public interface INotificationRepository : IRepository<Notification>
    {
        void Update(Notification obj);
    }
}
