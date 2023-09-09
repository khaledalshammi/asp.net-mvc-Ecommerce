using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ecommerece_Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IWebHostEnvironment webHostEnvironment, IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _db = db;
        }
        [HttpGet]
        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category,Images").ToList();
            return View(products);
        }
        [HttpGet]
        public ActionResult Details(int id)
        {
            if (id != 0 || id != null)
            {
                Product product = _unitOfWork.Product.Get(e => e.Id == id, includeProperties: "Category,Comments,Images,Reviews");
                List<Product> products = new List<Product>();
                foreach(int did in product.RelatedProductsId)
                {
                    products.Add(_unitOfWork.Product.Get(e => e.Id == did, includeProperties: "Images"));
                }
                RelatedProductVM relatedProductVM = new RelatedProductVM()
                {
                    Product = product,
                    RelatedProducts = products
                };
                return View(relatedProductVM);
            }
            return NotFound();
        }
        
        public ActionResult Create()
        {
            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i=> new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                Products = _unitOfWork.Product.GetAll().ToList()
            };
            return View(productVM);
        }
        [HttpPost]
        public ActionResult Create(ProductVM productVM, List<IFormFile> files, List<int>? ProductIds)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);
                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);
                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        Image productImage = new()
                        {
                            Url = @"\" + productPath + @"\" + fileName,
                            ProductId = productVM.Product.Id,
                        };
                        if (productVM.Product.Images == null)
                            productVM.Product.Images = new List<Image>();
                        productVM.Product.Images.Add(productImage);
                    }
                }
                if (ProductIds != null && ProductIds.Count>0)
                {
                    foreach (int did in ProductIds){
                        productVM.Product.RelatedProductsId.Add(did);
                    };
                }
                productVM.Product.CreatedAt = DateTime.Now;
                productVM.Product.UpdatedAt = DateTime.Now;
                _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
                //productVM = null;
                TempData["success"] = "Created successfully!";
                return RedirectToAction("Index");
            }
            return View(productVM);
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (id != 0 || id != null)
            {
                Product product = _unitOfWork.Product.Get(i=>i.Id == id, includeProperties: "Category,Comments,Images,Reviews");
                if (product != null)
                {
                    ProductVM productVM = new()
                    {
                        CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                        {
                            Text = i.Name,
                            Value = i.Id.ToString()
                        }),
                        Products = _unitOfWork.Product.GetAll(i=>i.Id != id).ToList()
                    };
                    productVM.Product = product;
                    return View(productVM);
                }
                return NotFound();
            }
            return NotFound();
        }
        [HttpPost]
        public ActionResult Edit(int PID, ProductVM productVM, List<IFormFile>? files, List<int>? ProductIds)
        {
            Product product = _unitOfWork.Product.Get(i => i.Id == PID, includeProperties: "Category,Comments,Images,Reviews");
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (files != null)
            {
                foreach (IFormFile file in files)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = @"images\products\product-" + product.Id;
                    string finalPath = Path.Combine(wwwRootPath, productPath);
                    if (!Directory.Exists(finalPath))
                        Directory.CreateDirectory(finalPath);
                    using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    Image productImage = new()
                    {
                        Url = @"\" + productPath + @"\" + fileName,
                        ProductId = product.Id,
                    };
                    if (product.Images == null)
                    {
                        product.Images = new List<Image>();
                    }
                    product.Images.Add(productImage);
                }
            }
            if (ProductIds != null && ProductIds.Count > 0)
            {
                product.RelatedProductsId = new List<int>();
                foreach (int did in ProductIds)
                {
                    product.RelatedProductsId.Add(did);
                };
            }
            product.UpdatedAt = DateTime.Now;
            product.Name = productVM.Product.Name;
            product.Description = productVM.Product.Description;
            product.Price = productVM.Product.Price;
            product.Quantity = productVM.Product.Quantity;
            product.CategoryId = productVM.Product.CategoryId;
            _unitOfWork.Product.Update(product);
            _unitOfWork.Save();
            TempData["success"] = "Updated successfully!";
            return RedirectToAction("Index");
        }
        public IActionResult DeleteImage(int imageId)
        {
            Image imageToBeDeleted = _unitOfWork.Image.Get(u => u.Id == imageId);
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.Url))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.Url.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.Image.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction("Edit", new {id = imageToBeDeleted.ProductId});
        }
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);
            if (Directory.Exists(finalPath))
            {
                List<string> filePaths = Directory.GetFiles(finalPath).ToList();
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }
                Directory.Delete(finalPath);
            }
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Deleted successfully!";
            return RedirectToAction("Index", "Product");
        }
    }
}
