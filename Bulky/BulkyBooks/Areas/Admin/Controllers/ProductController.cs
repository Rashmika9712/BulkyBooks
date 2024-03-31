using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBooks.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> obj = _unitOfWork.Product.GetAll().OrderBy(x => x.Title).ToList();
            return View(obj);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVm productVm = new ProductVm()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                //Crete
                return View(productVm);
            }
            else
            {
                //Update
                productVm.Product = _unitOfWork.Product.Get(x => x.Id == id);
                return View(productVm);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVm obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileNme = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileNme), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + fileNme;
                }

                _unitOfWork.Product.Add(obj.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                obj.CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });

                return View(obj);
            }
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Product does not exists!";
                return NotFound();
            }

            Product? product = _unitOfWork.Product.Get(x => x.Id == id);

            if (id == null)
            {
                TempData["error"] = "Product does not exists!";
                return NotFound();
            }

            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Product? product = _unitOfWork.Product.Get(x => x.Id == id);

            if (product == null)
            {
                TempData["error"] = "Product does not exists!";
                return NotFound();
            }

            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product Deleted Successfully!";
            return RedirectToAction("Index");
        }
    }
}
