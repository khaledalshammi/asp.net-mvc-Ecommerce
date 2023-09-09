using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerece_Web.Areas.Customer.Controllers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerece_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NotificationController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public NotificationController(IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Notification> notifications = _unitOfWork.Notification.GetAll(includeProperties: "User").ToList();
            return View(notifications);
        }
        public IActionResult Delete(int id)
        {
            if (id != 0 || id != null)
            {
                Notification notification = _unitOfWork.Notification.Get(i => i.Id == id);
                if (notification != null)
                {
                    _unitOfWork.Notification.Remove(notification);
                    _unitOfWork.Save();
                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            return NotFound();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Notification notification)
        {
            if(notification != null)
            {
                List<User> users = _unitOfWork.User.GetAll(u=>u.EnabledNotification == true).ToList();
                foreach (User user in users)
                {
                    notification.SentAt = DateTime.Now;
                    notification.User = user;
                    notification.UserId = user.Id;
                    _unitOfWork.Notification.Add(notification);
                    _unitOfWork.Save();
                    user.Notifications.Add(notification);
                    _unitOfWork.Save();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult EnableUserNotification(bool toggle)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            if (toggle)
            {
                user.EnabledNotification = true;
            }
            else
            {
                user.EnabledNotification = false;
            }
            _unitOfWork.Save();
            string refererUrl = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(refererUrl) || refererUrl.Contains("EnableUserNotification"))
            {
                return RedirectToAction("Index");
            }
            return Redirect(refererUrl);
        }
    }
}
