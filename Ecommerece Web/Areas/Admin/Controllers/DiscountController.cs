using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerece_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        public DiscountController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }
        public IActionResult Index(string valid)
        {
            if (valid == "true")
            {
                List<Discount> discounts = _unitOfWork.Discount.GetAll(i=>i.Available == true).ToList();
                return View(discounts);
            }
            else
            {
                List<Discount> discounts = _unitOfWork.Discount.GetAll().ToList();
                return View(discounts);
            }
        }
        public IActionResult Delete(int id)
        {
            Discount discount = _unitOfWork.Discount.Get(i => i.Id == id && i.Available == true);
            _unitOfWork.Discount.Remove(discount);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Discount discount)
        {
            if (discount != null)
            {
                bool checkDiscount = _db.Discounts.Any(e => e.Code == discount.Code);
                if (checkDiscount)
                {
                    ModelState.AddModelError("Code", "You Can't Create The Same Discount Code Twice");
                    return View(discount);
                }
                else
                {
                    _unitOfWork.Discount.Add(discount);
                    _unitOfWork.Save();
                    return RedirectToAction("Index");
                }
            }
            return View();
        }
    }
}
