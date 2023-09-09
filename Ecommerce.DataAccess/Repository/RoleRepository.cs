using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;

namespace Ecommerce.DataAccess.Repository
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        private ApplicationDbContext _db;
        public RoleRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Role obj)
        {
            _db.Roles.Update(obj);
        }
        public Role GetRole(string rolename)
        {
            return _db.Roles.FirstOrDefault(e => e.Title == rolename);
        }
    }
}
