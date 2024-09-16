using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

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
            List<Product> obj = _unitOfWork.Product.GetAll(includeProperties: "Category")
                                                   .OrderBy(x => x.Title).ToList();
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
                //Create
            }
            else
            {
                //Update
                productVm.Product = _unitOfWork.Product.Get(x => x.Id == id);
            }
            return View(productVm);
        }
        [HttpPost]
        public async ValueTask<IActionResult> Upsert(ProductVm obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrWhiteSpace(obj.Product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (obj.Product.Id == 0)
                {
                    if (file == null)
                    {
                        obj.Product.ImageUrl = string.Empty;
                    }
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                }
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

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> obj = _unitOfWork.Product.GetAll(includeProperties: "Category")
                                                   .OrderBy(x => x.Title).ToList();
            return Json(new { data = obj });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDelete = _unitOfWork.Product.Get(x => x.Id == id);
            if (productToBeDelete == null)
                return Json(new { success = false, message = "Error while deleting" });

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDelete.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
                System.IO.File.Delete(oldImagePath);

            _unitOfWork.Product.Remove(productToBeDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Deleted Successfully!" });
        }
        #endregion
    }
}
