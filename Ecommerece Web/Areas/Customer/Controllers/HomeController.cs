using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Ecommerece_Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string search)
        {
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                Subscription subscription = _unitOfWork.Subscription.Get(i => i.User == user);
                if (user.Subscription != null)
                {
                    if (subscription.End < DateTimeOffset.Now)
                    {
                        _unitOfWork.Subscription.Remove(subscription);
                        _unitOfWork.Save();
                        user.Subscription = null;
                        _unitOfWork.Save();
                    }
                }
            }
            if (search == null)
            {
                IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,Images");
                return View(productList);
            }
            else
            {
                IEnumerable<Product> productList = _unitOfWork.Product.GetAll(e=>e.Name.Contains(search) || e.Description.Contains(search), includeProperties: "Category,Images");
                return View(productList);
            }
        }
        public IActionResult Details(int id)
        {
            if (id != 0 || id != null)
            {
                bool exists = false;
                Review review = null;
                User user = null;
                Like like = null;
                DisLike disLike = null;
                List<Comment> comments = _unitOfWork.Comment.GetAll(u => u.ProductId == id, includeProperties: "Images,User").ToList();
                if (User.Identity.IsAuthenticated)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                    user = _unitOfWork.User.Get(u => u.Id == userId);
                    review = _unitOfWork.Review.Get(u => u.ProductId == id && u.UserId == userId);
                    like = _unitOfWork.Like.Get(u => u.ProductId == id && u.UserId==user.Id);
                    disLike = _unitOfWork.DisLike.Get(u => u.ProductId == id && u.UserId == user.Id);

                    Cart cart = _unitOfWork.Cart.Get(i => i.UserId == user.Id && i.Available == true);
                    ShoppingCart shoppingCart = _unitOfWork.ShoppingCart.Get(i => i.ProductId == id && i.User == user && i.Cart == cart);
                    if (shoppingCart != null)
                    {
                        exists = true;
                    }
                }
                Product product = _unitOfWork.Product.Get(e => e.Id == id, includeProperties: "Category,Comments,Images,Reviews");
                List<Product> products = new List<Product>();
                foreach (int did in product.RelatedProductsId)
                {
                    products.Add(_unitOfWork.Product.Get(e => e.Id == did, includeProperties: "Images"));
                }
                if (review != null)
                {
                    ProductReviewVM ProductReviewVM = new ProductReviewVM()
                    {
                        Product = product,
                        RelatedProducts = products,
                        Review = review,
                        User = user,
                        Like = like,
                        DisLike = disLike,
                        Comments = comments,
                        Exists = exists
                    };
                    return View(ProductReviewVM);
                }
                else
                {
                    ProductReviewVM ProductReviewVM = new ProductReviewVM()
                    {
                        Product = product,
                        RelatedProducts = products,
                        User = user,
                        Like = like,
                        DisLike = disLike,
                        Comments = comments,
                        Exists = exists
                    };
                    return View(ProductReviewVM);
                }
            }
            return NotFound();
        }
        [Authorize]
        public IActionResult Review (int PID)
        {
            ViewBag.PID = PID;
            return View();
        }
        [HttpPost]
        [Authorize]
        public IActionResult Review(string rate, int ProductId)
        {
            int r = int.Parse(rate);
            Product product = _unitOfWork.Product.Get(e => e.Id == ProductId);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);

            if (user != null && rate != null && ProductId != null)
            {
                Review review = new Review()
                {
                    Rate = r,
                    User = user,
                    UserId = userId,
                    Product = product,
                    ProductId = ProductId
                };
                _unitOfWork.Review.Add(review);
                _unitOfWork.Save();
                int totalrate = 0;
                foreach(Review rr in product.Reviews)
                {
                    totalrate += rr.Rate;
                }
                product.AVGReviews = (totalrate/product.Reviews.Count());
                _unitOfWork.Save();
                return RedirectToAction("Details", new { id = ProductId });
            }
            return View();
        }
        [Authorize]
        public IActionResult EditReview(int PID, string UID)
        {
            ViewBag.PID = PID;
            ViewBag.UID = UID;
            return View();
        }
        [HttpPost]
        [Authorize]
        public IActionResult EditReview(string rate, int PID, string UID)
        {
            if(rate != null || rate != "0")
            {
                int r = int.Parse(rate);
                Review review = _unitOfWork.Review.Get(e => e.ProductId == PID && e.UserId == UID);
                Product product = _unitOfWork.Product.Get(e => e.Id == PID);

                if (review != null)
                {
                    review.Rate = r;
                    _unitOfWork.Save();
                    int totalrate = 0;
                    foreach (Review rr in product.Reviews)
                    {
                        totalrate += rr.Rate;
                    }
                    product.AVGReviews = (totalrate / product.Reviews.Count());
                    _unitOfWork.Save();
                    return RedirectToAction("Details", new { id = PID });
                }
                return View();
            }
            return View();
        }
        [Authorize]
        public IActionResult Like(int PID)
        {
            Product product = _unitOfWork.Product.Get(u => u.Id == PID);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Like like = _unitOfWork.Like.Get(u => u.UserId == user.Id && u.ProductId == product.Id);
            DisLike disLike = _unitOfWork.DisLike.Get(u => u.UserId == user.Id && u.ProductId == product.Id);

            if (like != null)
            {
                product.Likes -= 1;
                _unitOfWork.Like.Remove(like);
                _unitOfWork.Save();
                return RedirectToAction("Details", new { id = PID });
            }
            else
            {
                if (disLike != null)
                {
                    product.DisLikes -= 1;
                    _unitOfWork.DisLike.Remove(disLike);
                    _unitOfWork.Save();
                }
                Like newlike = new Like()
                {
                    UserId = userId,
                    ProductId = PID,
                    Product = product,
                    User = user
                };
                product.Likes += 1;
                _unitOfWork.Like.Add(newlike);
                _unitOfWork.Save();
                return RedirectToAction("Details", new { id = PID });
            }
        }
        [Authorize]
        public IActionResult DisLike(int PID)
        {
            Product product = _unitOfWork.Product.Get(u => u.Id == PID);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Like like = _unitOfWork.Like.Get(u => u.UserId == user.Id && u.ProductId == PID);
            DisLike disLike = _unitOfWork.DisLike.Get(u => u.UserId == user.Id && u.ProductId == PID);

            if (disLike != null)
            {
                product.DisLikes -= 1;
                _unitOfWork.DisLike.Remove(disLike);
                _unitOfWork.Save();
                return RedirectToAction("Details", new { id = PID });
            }
            else
            {
                if (like != null)
                {
                    product.Likes -= 1;
                    _unitOfWork.Like.Remove(like);
                    _unitOfWork.Save();
                }
                DisLike newdislike = new DisLike()
                {
                    UserId = userId,
                    ProductId = PID,
                    Product = product,
                    User = user
                };
                product.DisLikes += 1;
                _unitOfWork.DisLike.Add(newdislike);
                _unitOfWork.Save();
                return RedirectToAction("Details", new { id = PID });
            }
        }
        [Authorize]
        [HttpPost]
        public IActionResult AddComment(int PID, string? message, List<IFormFile>? files)
        {
            Product product = _unitOfWork.Product.Get(u => u.Id == PID);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);

            if (PID != 0)
            {
                if (!string.IsNullOrEmpty(message) && files == null)
                {
                    Comment comment = new Comment() 
                    { 
                        User = user,
                        UserId = user.Id,
                        Product = product,
                        ProductId = PID,
                        Message = message,
                        CreatedAt = DateTimeOffset.Now,
                    };
                    _unitOfWork.Comment.Add(comment);
                    _unitOfWork.Save();
                    user.Comments.Add(comment);
                    _unitOfWork.Save();
                    product.Comments.Add(comment);
                    _unitOfWork.Save();
                }
                if (files != null && message == null)
                {
                    Comment comment = new Comment()
                    {
                        User = user,
                        Product = product,
                        CreatedAt = DateTimeOffset.Now
                    };
                    _unitOfWork.Comment.Add(comment);
                    _unitOfWork.Save();
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    if (files != null)
                    {
                        foreach (IFormFile file in files)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string commentPath = @"images\comments\product-" + product.Id +"\\"+ "comment-"+comment.Id;
                            string finalPath = Path.Combine(wwwRootPath, commentPath);
                            if (!Directory.Exists(finalPath)){
                                Directory.CreateDirectory(finalPath);
                            }
                            using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }
                            CommentImages commentImage = new()
                            {
                                Url = @"\" + commentPath + @"\" + fileName,
                                Comment = comment,
                                CommentId = comment.Id
                            };
                            comment.Images.Add(commentImage);
                        }
                    }
                    _unitOfWork.Comment.Update(comment);
                    _unitOfWork.Save();
                    user.Comments.Add(comment);
                    _unitOfWork.Save();
                    product.Comments.Add(comment);
                    _unitOfWork.Save();
                    TempData["success"] = "Created successfully!";
                    return RedirectToAction("Details", new { id = PID });
                }
                if (files != null && message != null)
                {
                    Comment comment = new Comment()
                    {
                        User = user,
                        UserId = user.Id,
                        Product = product,
                        ProductId = PID,
                        Message = message,
                        CreatedAt = DateTimeOffset.Now,
                    };
                    _unitOfWork.Comment.Add(comment);
                    _unitOfWork.Save();
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    if (files != null)
                    {
                        foreach (IFormFile file in files)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string commentPath = @"images\comments\product-" + product.Id + "\\" + "comment-" + comment.Id;
                            string finalPath = Path.Combine(wwwRootPath, commentPath);
                            if (!Directory.Exists(finalPath))
                            {
                                Directory.CreateDirectory(finalPath);
                            }
                            using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }
                            CommentImages commentImage = new()
                            {
                                Url = @"\" + commentPath + @"\" + fileName,
                                Comment = comment,
                                CommentId = comment.Id,
                            };
                            comment.Images.Add(commentImage);
                        }
                    }
                    _unitOfWork.Comment.Update(comment);
                    _unitOfWork.Save();
                    user.Comments.Add(comment);
                    _unitOfWork.Save();
                    product.Comments.Add(comment);
                    _unitOfWork.Save();
                    TempData["success"] = "Created successfully!";
                    return RedirectToAction("Details", new { id = PID });
                }
                }
            return View();
        }
        [Authorize]
        public IActionResult DeleteComment(int PID, int CID)
        {
            if (PID != 0 && CID != 0)
            {
                Product product = _unitOfWork.Product.Get(u => u.Id == PID);
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                Comment comment = _unitOfWork.Comment.Get(u => u.Id == CID && u.UserId==user.Id, includeProperties: "Images");
                if (comment != null)
                {
                    if(comment.Images.Count()>0 && comment.Images != null)
                    {
                        string commentPath = Path.Combine("images", "comments", "product-" + product.Id, "comment-" + comment.Id);
                        string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, commentPath);
                        if (Directory.Exists(finalPath))
                        {
                            List<string> filePaths = Directory.GetFiles(finalPath).ToList();
                            foreach (string filePath in filePaths)
                            {
                                System.IO.File.Delete(filePath);
                            }
                            Directory.Delete(finalPath);
                        }
                        List<CommentImages> commentImages = _unitOfWork.CommentImages.GetAll(e => e.CommentId == comment.Id).ToList();
                        _unitOfWork.CommentImages.RemoveRange(commentImages);
                        _unitOfWork.Save();
                    }
                    _unitOfWork.Comment.Remove(comment);
                    _unitOfWork.Save();
                    return RedirectToAction("Details", new { id = PID });
                }
            }
            return View();
        }
        public IActionResult Notification()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            List<Notification> notifications = _unitOfWork.Notification.GetAll(u=>u.UserId == user.Id).ToList();
            foreach(Notification notification in notifications)
            {
                notification.IsRead = true;
                _unitOfWork.Notification.Update(notification);
                _unitOfWork.Save();
            }
            return View(notifications);
        }
        public IActionResult News()
        {
            List<News> news = _unitOfWork.News.GetAll().ToList();
            return View(news);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}