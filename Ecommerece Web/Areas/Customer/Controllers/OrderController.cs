using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using MailKit.Search;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using Discount = Ecommerce.Models.Discount;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Subscription = Ecommerce.Models.Subscription;

namespace Ecommerece_Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public OrderController(IWebHostEnvironment webHostEnvironment, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            List<Order> orders = _unitOfWork.Order.GetAll(i=>i.Paid==true || i.Refund == true, includeProperties: "Cart.ShoppingCarts.Product").ToList();
            return View(orders);
        }
        [HttpGet]
        public IActionResult Order()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Order(Order order)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true, includeProperties: "User");
            Order myorder = _unitOfWork.Order.Get(u => u.Cart == cart);
            if (myorder == null)
            {
                order.OrderNumber = GenerateOrderNumber();
                order.Cart = cart;
                order.User = cart.User;
                if(cart.DiscountedPrice > 0)
                {
                    order.TotalPrice = cart.DiscountedPrice;
                }
                else
                {
                    order.TotalPrice = cart.TotalPrice;
                }
                order.Status = "Procussed";
                _unitOfWork.Order.Add(order);
                _unitOfWork.Save();
                return RedirectToAction("PayNow");
            }
            else
            {
                myorder.PhoneNumber = order.PhoneNumber;
                myorder.City = order.City;
                myorder.Address = order.Address;
                myorder.Email = order.Email;
                myorder.Cart = cart;
                if (cart.DiscountedPrice > 0)
                {
                    myorder.TotalPrice = cart.DiscountedPrice;
                }
                else
                {
                    myorder.TotalPrice = cart.TotalPrice;
                }
                _unitOfWork.Order.Update(myorder);
                _unitOfWork.Save();
                return RedirectToAction("PayNow");
            }
        }
        public IActionResult PayNow()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true, includeProperties: "User,ShoppingCarts.Product");
            Order myorder = _unitOfWork.Order.Get(u => u.Cart == cart);
            //stripe logic
            var domain = "https://localhost:44329/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Customer/Order/PaymentConfirmation",
                CancelUrl = domain + $"Customer/Home/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            /*
             foreach (var item in cart.ShoppingCarts)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.TotalPrice * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.Quantity
                };
                options.LineItems.Add(sessionLineItem);
            }
             */
            var totalAmountInCents = (long)(myorder.TotalPrice * 100);

            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = totalAmountInCents,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = cart.User.Name
                    }
                },
                Quantity = 1
            };
            options.LineItems.Add(sessionLineItem);
            var service = new SessionService();
            Session session = service.Create(options);
            myorder.SessionId = session.Id;
            myorder.PaymentIntentId = session.PaymentIntentId;
            myorder.Status = "unpaid";
            _unitOfWork.Order.Update(myorder);
            _unitOfWork.Save();
           
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true, includeProperties: "User,ShoppingCarts.Product");
            Discount discount = _unitOfWork.Discount.Get(i => i.Cart == cart);
            List<ShoppingCart> ourshoppingCart = _unitOfWork.ShoppingCart.GetAll(i => i.Cart == cart && i.User == user, includeProperties: "Product").ToList();

            Order myorder = _unitOfWork.Order.Get(u => u.Cart == cart);
            var service = new SessionService();
            Session session = service.Get(myorder.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                myorder.SessionId = session.Id;
                myorder.PaymentIntentId = session.PaymentIntentId;
                myorder.Status = "paid";
                myorder.Paid = true;
                myorder.PaidAt = DateTimeOffset.Now;
                myorder.ExpectedArrival = DateTimeOffset.Now.AddDays(8);
                _unitOfWork.Order.Update(myorder);
                _unitOfWork.Save();
                discount.Available = false;
                _unitOfWork.Discount.Update(discount);
                _unitOfWork.Save();
                foreach (ShoppingCart shopping in  ourshoppingCart)
                {
                    shopping.Product.Quantity -= shopping.Quantity;
                    _unitOfWork.Product.Update(shopping.Product);
                    _unitOfWork.Save();
                }
                cart.Available = false;
                _unitOfWork.Cart.Update(cart);
                _unitOfWork.Save();
                Cart newcart = new Cart()
                {
                    User = user,
                    Available = true,
                };
                _unitOfWork.Cart.Add(newcart);
                _unitOfWork.Save();
            }
            SendOrderDetails(myorder.Email, $"Your Order Number {myorder.OrderNumber}. Your order expected to arrive {myorder.ExpectedArrival.Value.ToString("MM/dd")}", "Successful payment!");
            HttpContext.Session.Clear();
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Arrive(int id)
        {
            Order order = _unitOfWork.Order.Get(i => i.Id == id, includeProperties: "Cart.ShoppingCarts.Product");
            order.Arrived = true;
            order.ArrivedAt = DateTimeOffset.Now;
            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();
            SendOrderDetails(order.Email, $"Your Order ({order.OrderNumber}) Has been arrived.", "Khaled Shop");
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Refund(int id)
        {
            Order order = _unitOfWork.Order.Get(i => i.Id == id, includeProperties: "Cart.ShoppingCarts.Product");
            var options = new RefundCreateOptions
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = order.PaymentIntentId
            };
            var service = new RefundService();
            Refund refund = service.Create(options);
            order.Refund = true;
            order.RefundedAt = DateTimeOffset.Now;
            order.Status = "refunded";
            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();
            foreach (ShoppingCart shoppingCart in order.Cart.ShoppingCarts)
            {
                shoppingCart.Product.Quantity += shoppingCart.Quantity;
                _unitOfWork.Product.Update(shoppingCart.Product);
                _unitOfWork.Save();
            }
            SendOrderDetails(order.Email, $"Your Order ({order.OrderNumber}) Has been Refunded.", "Refund Request");
            return RedirectToAction("Index");
        }
        [Authorize]
        public IActionResult Subscription(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId, includeProperties: "Subscription.Type");
            if (user.Subscription != null)
            {
                return NotFound();
            }
            SubscriptionType subscriptionType = _unitOfWork.SubscriptionType.Get(r => r.Id == id);
            var domain = "https://localhost:44329/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Customer/Order/SubscribeConfirmation?id={id}",
                CancelUrl = domain + $"Customer/Home/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            var totalAmountInCents = (long)(subscriptionType.Price * 100);

            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = totalAmountInCents,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = subscriptionType.Title
                    }
                },
                Quantity = 1
            };
            options.LineItems.Add(sessionLineItem);
            var service = new SessionService();
            Session session = service.Create(options);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        [Authorize]
        public IActionResult SubscribeConfirmation(int id)
        {
            string userCodes = "";
            SubscriptionType subscriptionType = _unitOfWork.SubscriptionType.Get(r => r.Id == id);
            for(int i=0; i < subscriptionType.NumberOfDiscount; i++)
            {
                userCodes += $"{i + 1}: ";
                string initalCode = GenerateDiscountCode();
                userCodes += initalCode;
                Discount discount = new Discount()
                {
                    Code = initalCode,
                    Available = true,
                    Percentage = subscriptionType.DiscountAmount
                };
                _unitOfWork.Discount.Add(discount);
                _unitOfWork.Save();
            }
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Subscription subscription = new Subscription()
            {
                Start = DateTimeOffset.Now,
                End = DateTimeOffset.Now.AddYears(1),
                Available = true,
                Type = subscriptionType,
                User = user,
            };
            _unitOfWork.Subscription.Add(subscription);
            _unitOfWork.Save();
            user.Subscription = subscription;
            _unitOfWork.User.Update(user);
            _unitOfWork.Save();
            SendOrderDetails(user.Email, $"You have successfully subscribed. Your Discount Codes: {userCodes}", $"{subscriptionType.Title} Subscription");
            return View();
        }
        public static string GenerateDiscountCode()
        {
            var random = new Random();
            string code = "";
            for (int i = 0; i < 2; i++)
            {
                code += random.Next();
            }
            code += DateTime.Now.ToString("yyyyMMdd");
            for (int i = 0; i < 2; i++)
            {
                code += (char)random.Next('A', 'Z' + 1);
            }
            return code;
        }
        public static string GenerateOrderNumber()
        {
            var random = new Random();
            string orderId = "";
            for (int i = 0; i < 5; i++)
            {
                orderId += random.Next();
            }
            orderId += DateTime.Now.ToString("yyyyMMddHHmmss");
            for (int i = 0; i < 5; i++)
            {
                orderId += (char)random.Next('A', 'Z' + 1);
            }
            return orderId;
        }
        private void SendOrderDetails(string UserEmail, string messageToSend, string subjectToSend)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            string smtpServer = configuration["SmtpSettings:SmtpServer"];
            int smtpPort = int.Parse(configuration["SmtpSettings:SmtpPort"]);
            string smtpUsername = configuration["SmtpSettings:SmtpUsername"];
            string smtpPassword = configuration["SmtpSettings:SmtpPassword"];
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Khaled Alshammi", "kkhhaa2002yl@gmail.com"));
            message.To.Add(new MailboxAddress("", UserEmail));
            message.Subject = subjectToSend;
            message.Body = new TextPart("plain") { Text = messageToSend };

            using (var client = new SmtpClient())
            {
                client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                client.Authenticate(smtpUsername, smtpPassword);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
