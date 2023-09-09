using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerece_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(IUnitOfWork unitOfWork, ApplicationDbContext db, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _roleManager = roleManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            List<Role> roles = _unitOfWork.Role.GetAll().ToList();
            return View(roles);
        }
        [HttpGet]
        public ActionResult Details(int id)
        {
            if (id != 0 || id != null)
            {
                Role role = _unitOfWork.Role.Get(e=>e.Id == id);
                return View(role);
            }
            return View();
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id != 0 || id != null)
            {
                Role role = _unitOfWork.Role.Get(e => e.Id == id);
                return View(role);
            }
            return NotFound();
        }
        [HttpPost]
        public ActionResult Edit(Role role, string valid)
        {
            if (!new[] { "0", "1", "Available", "NotAvailable" }.Contains(valid))
            {
                ModelState.AddModelError("Valid", "Should Be NotAvailable Or Available");
                return View(role);
            }

            bool Validationstr;
            if (valid == "Available" || valid == "1")
            {
                Validationstr = true;
            }
            else
            {
                Validationstr = false;
            }
            Role oldrole = _db.Roles.FirstOrDefault(u=>u.Id == role.Id);
            var oldtitle = oldrole.Title;
            bool checkrole2 = _db.Roles.Any(e => e.Title == role.Title);
            bool checkrole = _db.Roles.Any(e => e.Title == role.Title && e.Id==role.Id);
            if (checkrole || !checkrole2)
            {
                var identityRole = _roleManager.Roles.SingleOrDefault(r => r.Name == oldtitle);
                if (identityRole != null)
                {
                    identityRole.Name = role.Title;
                    _roleManager.UpdateAsync(identityRole).GetAwaiter().GetResult();
                }
                oldrole.Valid = Validationstr;
                oldrole.Title = role.Title;
                _unitOfWork.Save();
                TempData["success"] = "Updated successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("Title", "You Can't Create The Same Role Twice");
                return View(role);
            }
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Role role, string valid)
        {
            bool Validationstr;
            if (valid == "Available" || valid == "1")
            {
                Validationstr = true;
            }
            else
            {
                Validationstr = false;
            }
            bool checkrole = _db.Roles.Any(e => e.Title == role.Title);
            if (checkrole)
            {
                ModelState.AddModelError("Title", "You Can't Create The Same Role Twice");
                return View(role);
            }
            Role Myrole = new Role()
            {
                Title = role.Title,
                Valid = Validationstr
            };
            _unitOfWork.Role.Add(Myrole);
            _unitOfWork.Save();
            _roleManager.CreateAsync(new IdentityRole(role.Title)).GetAwaiter().GetResult();
            TempData["success"] = "Created successfully!";
            return RedirectToAction("Index", "Role");
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Role role = _unitOfWork.Role.Get(e => e.Id == id);
            return View(role);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            Role role = _unitOfWork.Role.Get(e => e.Id == id);
            var identityRole = _roleManager.Roles.SingleOrDefault(r => r.Name == role.Title);
            if (id == null || id == 0)
            {
                return NotFound();
            }
            _unitOfWork.Role.Remove(role);
            _unitOfWork.Save();
            _roleManager.DeleteAsync(identityRole).GetAwaiter().GetResult();
            TempData["success"] = "Deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
