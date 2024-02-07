using BulkyBooks.Data;
using BulkyBooks.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBooks.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<Category> obj = _db.Categories.OrderBy(x => x.DisplayOrder).ToList();
            return View(obj);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Category Created Successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Category does not exists!";
                return NotFound();
            }

            Category? category = _db.Categories.Find(id);

            if (id == null)
            {
                TempData["error"] = "Category does not exists!";
                return NotFound();
            }

            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Category Updated Successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Category does not exists!";
                return NotFound();
            }

            Category? category = _db.Categories.Find(id);

            if (id == null)
            {
                TempData["error"] = "Category does not exists!";
                return NotFound();
            }

            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Category? category = _db.Categories.Find(id);

            if (category == null)
            {
                TempData["error"] = "Category does not exists!";
                return NotFound();
            }

            _db.Categories.Remove(category);
            _db.SaveChanges();
            TempData["success"] = "Category Deleted Successfully!";
            return RedirectToAction("Index");
        }
    }
}
