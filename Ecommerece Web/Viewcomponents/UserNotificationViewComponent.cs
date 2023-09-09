using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerece_Web.Viewcomponents
{
    public class UserNotificationViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserNotificationViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            return View(user);
        }
    }
}
