using Net6.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Net6.Models;
using Microsoft.AspNetCore.Authorization;
using Net6.Utility;

namespace Net6Core.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> CategoryList = _unitOfWork.Category.GetAll();
            return View(CategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "shouldnt match");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Created Successfuly";
                return RedirectToAction("Index");
            }
            return View(obj);


        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var CategoryFromDb = _unitOfWork.Category.GetFirstOrDefault(q => q.Id == id);
            if (CategoryFromDb == null)
            {
                return NotFound();
            }
            return View(CategoryFromDb);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "shouldnt match");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Update Successfuly";

                return RedirectToAction("Index");
            }
            return View(obj);


        }

        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Category.GetFirstOrDefault(q => q.Id == id);

            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category Deleted Successfuly";

            return RedirectToAction("Index");

        }
    }
}
