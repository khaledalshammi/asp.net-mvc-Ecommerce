using Ecommerce.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using Ecommerce.DataAccess.Repository.IRepository;

namespace Ecommerce.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitOfWork;
        public DbInitializer(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db, IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _unitOfWork = unitOfWork;
        }
        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) { }
            Role Admin = _db.Roles.FirstOrDefault(e => e.Title == "Admin");
            if (Admin == null)
            {
                Role role = new Role { Title = "Admin", Valid = true };
//                _db.Roles.Add(role);
                _db.SaveChanges();
                _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                _userManager.CreateAsync(new User
                {
                    UserName = "admin@dotnetmastery.com",
                    Email = "admin@dotnetmastery.com",
                    Name = "Admin",
                    PostalCode = "+965",
                    PhoneNumber = "45423212",
                    EmailConfirmed = true,
                }, "Admin123*").GetAwaiter().GetResult();
                User user = _db.Users.FirstOrDefault(u => u.Email == "admin@dotnetmastery.com");
                Cart cart = new Cart()
                {
                    User = user,
                    Available = true,
                };
                _unitOfWork.Cart.Add(cart);
                _unitOfWork.Save();
                user.Role.Add(role);
                _db.SaveChanges();
                _userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
            }
            return;
        }
    }
}
