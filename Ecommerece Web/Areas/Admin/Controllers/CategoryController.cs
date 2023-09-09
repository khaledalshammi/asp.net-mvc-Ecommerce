using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Ecommerece_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(IUnitOfWork unitOfWork, ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Category> categories = _unitOfWork.Category.GetAll(includeProperties: "Products").ToList();
            return View(categories);
        }
        [HttpGet]
        public ActionResult Details(int id)
        {
            if (id != 0 || id != null)
            {
                Category category = _unitOfWork.Category.Get(e => e.Id == id);
                if (category != null)
                {
                    return View(category);
                }
                else
                {
                    return NotFound();
                }
            }
            return NotFound();
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Category category, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string categoryPath = Path.Combine(wwwRootPath, @"images/category");
                    if (!Directory.Exists(categoryPath))
                        Directory.CreateDirectory(categoryPath);
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, category.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    category.ImageUrl = @"images/category/" + fileName;
                }
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "Created successfully!";
                return RedirectToAction("Index");
                }
                return View();
         }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id != 0 || id != null)
            {
                Category category = _unitOfWork.Category.Get(e => e.Id == id);
                return View(category);
            }
            return NotFound();
        }
        [HttpPost]
        public ActionResult Edit(Category category, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                Category mycategory = _unitOfWork.Category.Get(r=>r.Id == category.Id);
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images/category");
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, category.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    category.ImageUrl = @"images/category/" + fileName;
                    mycategory.ImageUrl = category.ImageUrl;
                }
                mycategory.Name = category.Name;
                mycategory.Description = category.Description;
                _unitOfWork.Save();
                TempData["success"] = "Updated successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category category = _unitOfWork.Category.Get(e => e.Id == id);
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            Category category = _unitOfWork.Category.Get(e => e.Id == id);
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var oldImagePath =
                           Path.Combine(_webHostEnvironment.WebRootPath,
                           category.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Category.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "Deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
