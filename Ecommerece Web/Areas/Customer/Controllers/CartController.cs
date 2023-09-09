using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerece_Web.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CartController(IWebHostEnvironment webHostEnvironment, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpPost]
        public IActionResult AddToCart(int PID)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Cart cart = _unitOfWork.Cart.Get(i=>i.UserId == user.Id && i.Available == true);
            Product product = _unitOfWork.Product.Get(i => i.Id == PID);
            ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(i => i.ProductId == product.Id && i.User.Id == user.Id && i.Cart.Id == cart.Id);
            if (product != null && shoppingCart == null)
            {
                if (product.Quantity > 1)
                {
                    ShoppingCart newshoppingCart = new ShoppingCart()
                    {
                        User = user,
                        Cart = cart,
                        Product = product,
                        ProductId = product.Id,
                        Quantity  = 1,
                        TotalPrice = product.Price
                    };
                    _unitOfWork.ShoppingCart.Add(newshoppingCart);
                    _unitOfWork.Save();
                    cart.ShoppingCarts.Add(newshoppingCart);
                    cart.TotalQuantity += 1;
                    cart.TotalPrice += product.Price;
                    _unitOfWork.Cart.Update(cart);
                    _unitOfWork.Save();
                    return RedirectToAction("Details","Home", new { id = product.Id });
                }
            }
            return RedirectToAction("Details", "Home", new { id = product.Id });
        }
        public IActionResult Cart(string valid)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId, includeProperties: "Subscription.Type");
            Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true, includeProperties: "ShoppingCarts");
            List<ShoppingCart> ourshoppingCart = _unitOfWork.ShoppingCart.GetAll(i=>i.Cart == cart && i.User == user, includeProperties: "Product").ToList();

            int totalQuantity = 0;
            double totalPrice = 0;
            foreach(ShoppingCart shoppingCart in ourshoppingCart)
            {
                int quantity = 0;
                if (shoppingCart.Product.Quantity < shoppingCart.Quantity)
                {
                    quantity += shoppingCart.Product.Quantity;
                }
                else
                {
                    quantity += shoppingCart.Quantity;
                }
                
                if (quantity > 0)
                {
                    shoppingCart.Quantity = quantity;
                    shoppingCart.TotalPrice = quantity * shoppingCart.Product.Price;
                    _unitOfWork.ShoppingCart.Update(shoppingCart);
                    _unitOfWork.Save();
                    totalQuantity += shoppingCart.Quantity;
                    totalPrice += shoppingCart.TotalPrice;
                }
                else
                {
                    _unitOfWork.ShoppingCart.Remove(shoppingCart);
                    _unitOfWork.Save();
                }
            }
            cart.TotalQuantity = totalQuantity;
            cart.TotalPrice = totalPrice;
            _unitOfWork.Cart.Update(cart);
            _unitOfWork.Save();
            if (user.Subscription != null)
            {
                int initial = user.Subscription.Type.CartDiscountPercentage;
                double cartTotalPrice = cart.TotalPrice;
                decimal decimalinitial = (decimal)initial / 100;
                cart.TotalPrice = cartTotalPrice - (cartTotalPrice * (double)decimalinitial);
                _unitOfWork.Cart.Update(cart);
                _unitOfWork.Save();
            }
            CartVM cartVM = new CartVM()
            {
                Cart = cart,
                ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(i => i.Cart == cart && i.User == user, includeProperties: "Product.Images").ToList(),
                DiscountCode = _unitOfWork.Discount.Get(i => i.Cart == cart),
                ValidCode = valid,
                User = user
            };
            return View(cartVM);
        }
        public IActionResult Plus(int PID)
        {
            if (PID != 0 || PID != null)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true);
                Product product = _unitOfWork.Product.Get(i => i.Id == PID);
                if (product != null)
                {
                    ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(i => i.ProductId == PID && i.Cart == cart && user == user, includeProperties: "Product");
                    shoppingCart.Quantity += 1;
                    shoppingCart.TotalPrice += product.Price;
                    _unitOfWork.ShoppingCart.Update(shoppingCart);
                    _unitOfWork.Save();
                    return RedirectToAction("Cart");
                }
                return RedirectToAction("Cart");
            }
            return RedirectToAction("Cart");
        }
        public IActionResult Minus(int PID)
        {
            if (PID != 0 || PID != null)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true);
                Product product = _unitOfWork.Product.Get(i => i.Id == PID);
                if (product != null)
                {
                    ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(i => i.ProductId == PID && i.Cart == cart && user == user);
                    if(shoppingCart.Quantity == 1)
                    {
                        _unitOfWork.ShoppingCart.Remove(shoppingCart);
                        _unitOfWork.Save();
                        return RedirectToAction("Cart");
                    }
                    else
                    {
                        shoppingCart.Quantity -= 1;
                        shoppingCart.TotalPrice -= product.Price;
                        _unitOfWork.ShoppingCart.Update(shoppingCart);
                        _unitOfWork.Save();
                        return RedirectToAction("Cart");
                    }
                }
                return RedirectToAction("Cart");
            }
            return RedirectToAction("Cart");
        }
        public IActionResult Delete(int PID)
        {
            if (PID != 0 || PID != null)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true);
                Product product = _unitOfWork.Product.Get(i => i.Id == PID);
                if (product != null)
                {
                    ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(i => i.ProductId == PID && i.Cart == cart && user == user);
                    _unitOfWork.ShoppingCart.Remove(shoppingCart);
                    _unitOfWork.Save();
                    return RedirectToAction("Cart");
                }
                return RedirectToAction("Cart");
            }
            return RedirectToAction("Cart");
        }
        [HttpPost]
        public IActionResult DiscountCode(string code)
        {
            Discount discount = _unitOfWork.Discount.Get(i=>i.Code == code && i.Available == true);
            if (discount == null)
            {
                return RedirectToAction("Cart", new {valid = "false"});
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true);
                discount.Cart = cart;
                _unitOfWork.Discount.Update(discount);
                _unitOfWork.Save();
                int initial = discount.Percentage;
                double cartTotalPrice = cart.TotalPrice;
                decimal decimalinitial = (decimal)initial/100;

                cart.DiscountedPrice = cartTotalPrice - (cartTotalPrice * (double)decimalinitial);
                _unitOfWork.Cart.Update(cart);
                _unitOfWork.Save();
            }
            return RedirectToAction("Cart");
        }
        public IActionResult CancelDiscountCode(int id)
        {
            if (id != 0 || id != null)
            {
                Discount discount = _unitOfWork.Discount.Get(i => i.Id == id && i.Available == true, includeProperties:"Cart");
                if (discount != null)
                {
                    discount.Cart.DiscountedPrice = null;
                    _unitOfWork.Cart.Update(discount.Cart);
                    _unitOfWork.Save();
                    discount.Cart = null;
                    _unitOfWork.Discount.Update(discount);
                    _unitOfWork.Save();
                    return RedirectToAction("Cart");
                }
                return View();
            }
            return View();
        }
    }
}
