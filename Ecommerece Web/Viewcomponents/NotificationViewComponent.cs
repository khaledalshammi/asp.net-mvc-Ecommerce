using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerece_Web.Viewcomponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotificationViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            int notification = _unitOfWork.Notification.GetAll(u => u.UserId == user.Id && u.IsRead == false).Count();
            return View(notification);
        }
    }
}
