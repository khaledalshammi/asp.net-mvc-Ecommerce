using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerece_Web.Viewcomponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Cart cart = _unitOfWork.Cart.Get(u => u.User == user && u.Available == true);
            if (cart.TotalQuantity > 0)
            {
                return View(cart.TotalQuantity);
            }
            else
            {
                return View(0);
            }
        }
    }
}
