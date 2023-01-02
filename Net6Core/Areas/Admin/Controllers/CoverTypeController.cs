using Net6.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Net6.Models;
using Microsoft.AspNetCore.Authorization;
using Net6.Utility;

namespace Net6Core.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> coverTypes = _unitOfWork.CoverType.GetAll();
            return View(coverTypes);
        }

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Cover Type Added Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion

        #region Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
           if(ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Cover Type Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion

        #region Delete

        public IActionResult Delete(int? id)
        {
            var CoverTypeFromDb = _unitOfWork.CoverType.GetFirstOrDefault(q => q.Id == id);

            if (CoverTypeFromDb == null)
            {
                return NotFound();
            }

            _unitOfWork.CoverType.Remove(CoverTypeFromDb);
            _unitOfWork.Save();
            TempData["success"] = "CoverType Deleted Successfuly";

            return RedirectToAction("Index");
        }
        #endregion
    }
}
