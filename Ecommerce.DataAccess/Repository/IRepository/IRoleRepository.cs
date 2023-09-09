using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository.IRepository
{
    public interface IRoleRepository : IRepository<Role>
    {
        void Update(Role obj);
        Role GetRole(string rolename);
    }
}
