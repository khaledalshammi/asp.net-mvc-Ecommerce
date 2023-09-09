using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using MailKit.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Stripe;
using System.Security.Claims;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Ecommerece_Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class InquiryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public InquiryController(IWebHostEnvironment webHostEnvironment, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        [Authorize(Roles = "Admin")]

        public IActionResult Index()
        {
            List<Inquiry> inquiries =  _unitOfWork.Inquiry.GetAll(includeProperties:"User").ToList();
            return View(inquiries);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Details(int id)
        {
            if (id != 0 || id != null)
            {
                Inquiry inquiry = _unitOfWork.Inquiry.Get(i => i.Id == id, includeProperties: "User");
                if (inquiry != null)
                {
                    return View(inquiry);
                }
                return NotFound();
            }
            return NotFound();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Close(int id)
        {
            if (id != 0 || id != null)
            {
                Inquiry inquiry = _unitOfWork.Inquiry.Get(i => i.Id == id, includeProperties: "User");
                if (inquiry != null)
                {
                    inquiry.Closed = true;
                    inquiry.ClosedAt = DateTimeOffset.Now;
                    _unitOfWork.Inquiry.Update(inquiry);
                    _unitOfWork.Save();
                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            return NotFound();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Reply(int id)
        {
            if (id != 0 || id != null)
            {
                Inquiry inquiry = _unitOfWork.Inquiry.Get(i => i.Id == id, includeProperties: "User");
                if (inquiry != null )
                {
                    return View(inquiry);
                }
                return NotFound();
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Reply(int id, string reply)
        {
            if (id != 0 || id != null || reply != null)
            {
                Inquiry inquiry = _unitOfWork.Inquiry.Get(i => i.Id == id, includeProperties: "User");
                if (inquiry != null)
                {
                    inquiry.Closed = true;
                    inquiry.ClosedAt = DateTimeOffset.Now;
                    inquiry.Reply = true;
                    _unitOfWork.Inquiry.Update(inquiry);
                    _unitOfWork.Save();
                    var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                    string smtpServer = configuration["SmtpSettings:SmtpServer"];
                    int smtpPort = int.Parse(configuration["SmtpSettings:SmtpPort"]);
                    string smtpUsername = configuration["SmtpSettings:SmtpUsername"];
                    string smtpPassword = configuration["SmtpSettings:SmtpPassword"];
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Khaled Alshammi", "kkhhaa2002yl@gmail.com"));
                    message.To.Add(new MailboxAddress(null, inquiry.User.Email));
                    message.Subject = "Hello" + inquiry.User.Name;
                    message.Body = new TextPart("plain") { Text = reply };
                    using (var client = new SmtpClient())
                    {
                        client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                        client.Authenticate(smtpUsername, smtpPassword);
                        client.Send(message);
                        client.Disconnect(true);
                    }
                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult Create(Inquiry inquiry, string type, IFormFile? file)
        {
            if (inquiry.Content != null)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                if (type == null)
                {
                    return View();
                }
                if (type != null)
                {
                    inquiry.User = user;
                    inquiry.UserId = user.Id;
                    inquiry.CreatedAt = DateTimeOffset.Now;
                    inquiry.Type = type;
                }
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string newsPath = Path.Combine(wwwRootPath, @"images/Inquiries");
                    if (!Directory.Exists(newsPath))
                    {
                        Directory.CreateDirectory(newsPath);
                    }
                    if (!string.IsNullOrEmpty(inquiry.ImgUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, inquiry.ImgUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(newsPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    inquiry.ImgUrl = @"images/Inquiries/" + fileName;
                }
                _unitOfWork.Inquiry.Add(inquiry);
                _unitOfWork.Save();
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}
