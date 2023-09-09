using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using MailKit.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;
using MimeKit;
using System;
using System.Web.Mvc;
using System.Security.Claims;
using System.Security.Policy;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Ecommerece_Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class JobController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public JobController(IWebHostEnvironment webHostEnvironment, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            IEnumerable<Job> jobs = _unitOfWork.Job.GetAll(i => i.Available == true, includeProperties: "Applicants").ToList();
            if (User.Identity.IsAuthenticated)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                ViewBag.User = user;
            }
            return View(jobs);
        }

        public IActionResult Details(int id)
        {
            if (id != 0 || id != null)
            {
                string referer = Request.Headers["Referer"].ToString();
                Job job = _unitOfWork.Job.Get(i => i.Id == id);
                if (job != null)
                {
                    if (!string.IsNullOrEmpty(referer))
                    {
                        if (referer.Contains("MyJobs"))
                        {
                            ViewBag.url = "MyJobs";
                        }
                    }
                    return View(job);
                }
                return NotFound();
            }
            return NotFound();
        }
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                if (referer.Contains("MyJobs"))
                {
                    ViewBag.url = "MyJobs";
                }
            }

            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult Create(Job job, IFormFile? file, string available, string url)
        {
            if (available == null)
            {
                ModelState.AddModelError("Available", "Couldn't be null");
                return View();
            }
            if (job != null)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                job.User = user;
                job.UserId = user.Id;
                job.CreatedAt = DateTimeOffset.Now;
                job.Available = (available == "1") ? true : false;
                _unitOfWork.Job.Add(job);
                _unitOfWork.Save();
                user.Jobs.Add(job);
                _unitOfWork.Save();

                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string newsPath = Path.Combine(wwwRootPath, @"images/Jobs");
                    if (!Directory.Exists(newsPath))
                    {
                        Directory.CreateDirectory(newsPath);
                    }
                    if (!string.IsNullOrEmpty(job.Image))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, job.Image.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(newsPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    job.Image = @"images/Jobs/" + fileName;
                }
                _unitOfWork.Job.Update(job);
                _unitOfWork.Save();
                TempData["success"] = "Created successfully!";
                if (url != null)
                {
                    return RedirectToAction(url);
                }
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            if (id != 0 || id != null)
            {
                string referer = Request.Headers["Referer"].ToString();
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                Job job = _unitOfWork.Job.Get(i => i.Id == id, includeProperties:"User");
                if (job.UserId == user.Id || User.IsInRole("Admin"))
                {
                    if (!string.IsNullOrEmpty(referer))
                    {
                        if (referer.Contains("MyJobs"))
                        {
                            ViewBag.url = "MyJobs";
                        }
                    }
                    return View(job);
                }
                return NotFound();
            }
            return NotFound();
        }
        [HttpPost]
        [Authorize]
        public IActionResult Edit(string JobId, Job newJob, IFormFile? file, string available, string url)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);
            Job job = _unitOfWork.Job.Get(u => u.Id.ToString() == JobId);
            if (job.UserId == user.Id || User.IsInRole("Admin"))
            {
                if (job != null)
                {
                    job.CreatedAt = DateTimeOffset.Now;
                    if (available != null)
                    {
                        job.Available = (available == "1") ? true : false;
                    }
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string newsPath = Path.Combine(wwwRootPath, @"images/Jobs");
                        if (!Directory.Exists(newsPath))
                        {
                            Directory.CreateDirectory(newsPath);
                        }
                        if (!string.IsNullOrEmpty(job.Image))
                        {
                            var oldImagePath = Path.Combine(wwwRootPath, job.Image.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        using (var fileStream = new FileStream(Path.Combine(newsPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        job.Image = @"images/Jobs/" + fileName;
                    }
                    job.Title = newJob.Title;
                    job.Position = newJob.Position;
                    job.Requirement = newJob.Requirement;
                    job.Description = newJob.Description;
                    job.Hours = newJob.Hours;
                    job.Salary = newJob.Salary;
                    job.From = newJob.From;
                    job.To = newJob.To;
                    job.City = newJob.City;
                    job.Country = newJob.Country;
                    job.Address = newJob.Address;
                    job.Days = newJob.Days;
                    job.Off = newJob.Off;
                    job.Location = newJob.Location;
                    _unitOfWork.Job.Update(job);
                    _unitOfWork.Save();
                    TempData["success"] = "Updated successfully!";
                    if (url != null)
                    {
                        return RedirectToAction(url);
                    }
                    return RedirectToAction("Index");
                }
                return NotFound();
            }
            return View();
        }
        [Authorize]
        public IActionResult Delete(int id)
        {
            if (id != 0 || id != null)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                User user = _unitOfWork.User.Get(u => u.Id == userId);
                Job job = _unitOfWork.Job.Get(i => i.Id == id);
                string referer = Request.Headers["Referer"].ToString();
                if (job.UserId == user.Id || User.IsInRole("Admin"))
                {
                    if(job.Image != null)
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, job.Image.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    _unitOfWork.Job.Remove(job);
                    _unitOfWork.Save();
                    TempData["success"] = "Deleted successfully!";
                    if (!string.IsNullOrEmpty(referer))
                    {
                        if (referer.Contains("MyJobs"))
                        {
                            return RedirectToAction("MyJobs");
                        }
                    }
                    return RedirectToAction("Index");
                }
                return View();
            }
            return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult Apply(int JID)
        {
            if (JID != 0 || JID != null)
            {
                Job job = _unitOfWork.Job.Get(i => i.Id == JID);
                return View(job);
            }
            return NotFound();
        }
        [Authorize]
        [HttpPost]
        public IActionResult Apply(int JID, IFormFile? CV)
        {
            if (JID != 0 && JID != null && CV != null)
            {
                Job job = _unitOfWork.Job.Get(i => i.Id == JID);
                if (job != null)
                {
                    string fileExtension = Path.GetExtension(CV.FileName).ToLower();
                    if (fileExtension == ".pdf" || fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                        string smtpServer = configuration["SmtpSettings:SmtpServer"];
                        int smtpPort = int.Parse(configuration["SmtpSettings:SmtpPort"]);
                        string smtpUsername = configuration["SmtpSettings:SmtpUsername"];
                        string smtpPassword = configuration["SmtpSettings:SmtpPassword"];

                        var claimsIdentity = (ClaimsIdentity)User.Identity;
                        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                        User user = _unitOfWork.User.Get(u => u.Id == userId);

                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string fileName = "";
                        if (CV != null)
                        {
                            fileName = Guid.NewGuid().ToString() + Path.GetExtension(CV.FileName);
                            string newsPath = Path.Combine(wwwRootPath, @"files/cvs");
                            if (!Directory.Exists(newsPath))
                            {
                                Directory.CreateDirectory(newsPath);
                            }
                            using (var fileStream = new FileStream(Path.Combine(newsPath, fileName), FileMode.Create))
                            {
                                CV.CopyTo(fileStream);
                            }
                        }
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("Khaled Alshammi", "kkhhaa2002yl@gmail.com"));
                        message.To.Add(new MailboxAddress(null, user.Email));
                        message.Subject = "Hello" + user.Name;
                        message.Body = new TextPart("plain") { Text = "You have applied successfully" };
                        Applicant applicant = new Applicant()
                        {
                            User = user,
                            Job = job,
                            //CV = @"files/cvs/" + fileName,
                            CV = Path.Combine("files/cvs", fileName),
                            AppliedAt = DateTimeOffset.Now
                        };
                        _unitOfWork.Applicant.Add(applicant);
                        _unitOfWork.Save();
                        user.Applicants.Add(applicant);
                        _unitOfWork.Save();
                        job.Applicants.Add(applicant);
                        _unitOfWork.Save();

                        using (var client = new SmtpClient())
                        {
                            client.Connect(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                            client.Authenticate(smtpUsername, smtpPassword);
                            client.Send(message);
                            client.Disconnect(true);
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.CVError = "Only PDF and Excel files are allowed.";
                        return RedirectToAction("Apply", new { JID = job.Id });
                    }
                }
                return RedirectToAction("Apply", new { JID = job.Id });
            }
            return NotFound();
        }
        [Authorize]
        public IActionResult MyJobs()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);

            IEnumerable<Job> jobs = _unitOfWork.Job.GetAll(i => i.UserId == user.Id, includeProperties: "Applicants").ToList();
            if (jobs != null)
            {
                return View(jobs);
            }
            return NotFound();
        }
        [Authorize]
        public IActionResult JobApplicants(int JobId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            User user = _unitOfWork.User.Get(u => u.Id == userId);

            Job job = _unitOfWork.Job.Get(i => i.UserId == user.Id && i.Id == JobId, includeProperties: "Applicants");
            List<Applicant> applicants = _unitOfWork.Applicant.GetAll(i => i.Job == job, includeProperties: "User").ToList();
            return View(applicants);
        }
    }
}
