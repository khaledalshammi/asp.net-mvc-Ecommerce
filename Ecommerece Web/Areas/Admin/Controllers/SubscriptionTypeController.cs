using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerece_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubscriptionTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public SubscriptionTypeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            List<SubscriptionType> subscriptionTypes = _unitOfWork.SubscriptionType.GetAll().ToList();
            return View(subscriptionTypes);
        }
        [HttpGet]
        [Authorize]
        public ActionResult Details()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId, includeProperties: "Subscription.Type");
            SubscriptionTypeVM subscriptionTypeVM = new SubscriptionTypeVM()
            {
                SubscriptionTypes = _unitOfWork.SubscriptionType.GetAll().ToList(),
                User = user
            };
            return View(subscriptionTypeVM);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create(SubscriptionType subscriptionType, IFormFile? file)
        {
            if (subscriptionType != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string categoryPath = Path.Combine(wwwRootPath, @"images/subscriptionType");
                    if (!Directory.Exists(categoryPath))
                        Directory.CreateDirectory(categoryPath);
                    if (!string.IsNullOrEmpty(subscriptionType.Image))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, subscriptionType.Image.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    subscriptionType.Image = @"images/subscriptionType/" + fileName;
                }
                _unitOfWork.SubscriptionType.Add(subscriptionType);
                _unitOfWork.Save();
                TempData["success"] = "Created successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id != 0 || id != null)
            {
                SubscriptionType subscriptionType = _unitOfWork.SubscriptionType.Get(e => e.Id == id);
                return View(subscriptionType);
            }
            return NotFound();
        }
        [HttpPost]
        public ActionResult Edit(SubscriptionType subscriptionType, IFormFile? file)
        {
            if (subscriptionType != null)
            {
                SubscriptionType subscriptionTypeOld = _unitOfWork.SubscriptionType.Get(r => r.Id == subscriptionType.Id);
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images/subscriptionType");
                    if (!string.IsNullOrEmpty(subscriptionType.Image))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, subscriptionType.Image.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    subscriptionType.Image = @"images/subscriptionType/" + fileName;
                    subscriptionTypeOld.Image = subscriptionType.Image;
                }
                subscriptionTypeOld.Title = subscriptionType.Title;
                subscriptionTypeOld.Price = subscriptionType.Price;
                subscriptionTypeOld.NumberOfDiscount = subscriptionType.NumberOfDiscount;
                subscriptionTypeOld.DiscountAmount = subscriptionType.DiscountAmount;
                subscriptionTypeOld.Description = subscriptionType.Description;
                subscriptionTypeOld.CartDiscountPercentage = subscriptionType.CartDiscountPercentage;
                _unitOfWork.Save();
                TempData["success"] = "Updated successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            SubscriptionType subscriptionType = _unitOfWork.SubscriptionType.Get(e => e.Id == id);
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, subscriptionType.Image.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.SubscriptionType.Remove(subscriptionType);
            _unitOfWork.Save();
            TempData["success"] = "Deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
