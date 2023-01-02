using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net6.DataAccess.Repository.IRepository;
using Net6.Models;
using Net6.Utility;

namespace Net6Core.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Company> company = _unitOfWork.Company.GetAll();

            return View(company);
        }

        public IActionResult Upsert(int? id)
        {
            Company company = new();

            if(id==0 || id == null)
            {
                return View(company);
            }
            else
            {
                company = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
                return View(company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["Success"] = "Product created Successfully";

                }
                else
                {
                    _unitOfWork.Company.Update(obj);
                    TempData["Success"] = "Product Updated Successfully";

                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new {data = companyList});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj= _unitOfWork.Company.GetFirstOrDefault(u=>u.Id== id);
            if (obj == null)
            {
                return Json(new {success=false , message = "Error While Deleting"});
            }
            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }
    }
}
