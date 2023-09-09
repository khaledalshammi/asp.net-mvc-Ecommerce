using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerece_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NewsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(IUnitOfWork unitOfWork, ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public IActionResult Index()
        {
            IEnumerable<News> news = _unitOfWork.News.GetAll().OrderByDescending(o => o.CreatedAt).ToList();
            return View(news);
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            if (id != 0)
            {
                News news = _unitOfWork.News.Get(e => e.Id == id);
                return View(news);
            }
            return NotFound();
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(News news, IFormFile? file, IFormFile? video)
        {
            news.CreatedAt = DateTimeOffset.Now;
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string newsPath = Path.Combine(wwwRootPath, @"images/news/images");
                if (!Directory.Exists(newsPath))
                {
                    Directory.CreateDirectory(newsPath);
                }
                if (!string.IsNullOrEmpty(news.ImageUrl))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, news.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                using (var fileStream = new FileStream(Path.Combine(newsPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                news.ImageUrl = @"images/news/images/" + fileName;
            }
            if (video != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
                string newsPath = Path.Combine(wwwRootPath, @"images/news/videos");
                if (!Directory.Exists(newsPath))
                {
                    Directory.CreateDirectory(newsPath);
                }
                if (!string.IsNullOrEmpty(news.VideoUrl))
                {
                    var oldVideoPath = Path.Combine(wwwRootPath, news.VideoUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldVideoPath))
                    {
                        System.IO.File.Delete(oldVideoPath);
                    }
                }
                using (var fileStream = new FileStream(Path.Combine(newsPath, fileName), FileMode.Create))
                {
                    video.CopyTo(fileStream);
                }
                news.VideoUrl = @"images/news/videos/" + fileName;
            }
            _unitOfWork.News.Add(news);
            _unitOfWork.Save();
            TempData["success"] = "Created successfully!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id != 0 || id != null)
            {
                News news = _unitOfWork.News.Get(e => e.Id == id);
                return View(news);
            }
            return NotFound();
        }
        [HttpPost]
        public ActionResult Edit(News news, IFormFile? file, IFormFile? video)
        {
            if (ModelState.IsValid)
            {
                News news2 = _unitOfWork.News.Get(r => r.Id == news.Id);
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string newsPath = Path.Combine(wwwRootPath, @"images/news/images");
                    if (!string.IsNullOrEmpty(news.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, news.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(newsPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    news.ImageUrl = @"images/news/images/" + fileName;
                    news2.ImageUrl = news.ImageUrl;
                }
                if (video != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
                    string newsPath = Path.Combine(wwwRootPath, @"images/news/videos");
                    if (!string.IsNullOrEmpty(news.VideoUrl))
                    {
                        // delete the old video
                        var oldVideoPath = Path.Combine(wwwRootPath, news.VideoUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldVideoPath))
                        {
                            System.IO.File.Delete(oldVideoPath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(newsPath, fileName), FileMode.Create))
                    {
                        video.CopyTo(fileStream);
                    }
                    news.VideoUrl = @"images/news/videos/" + fileName;
                    news2.VideoUrl = news.VideoUrl;
                }
                news2.Title = news.Title;
                news2.Content = news.Content;
                news2.Url = news.Url;
                news2.UrlDescription = news.UrlDescription;
                _unitOfWork.Save();
                TempData["success"] = "Updated successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int id)
        {
            News news = _unitOfWork.News.Get(e => e.Id == id);
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, news.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            var oldVideoPath = Path.Combine(_webHostEnvironment.WebRootPath, news.VideoUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldVideoPath))
            {
                System.IO.File.Delete(oldVideoPath);
            }
            _unitOfWork.News.Remove(news);
            _unitOfWork.Save();
            TempData["success"] = "Deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
