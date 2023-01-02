using Microsoft.AspNetCore.Mvc;
using Net6.DataAccess.Repository;
using Net6.DataAccess.Repository.IRepository;
using Net6.Utility;
using System.Security.Claims;

namespace Net6Core.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if(HttpContext.Session.GetInt32(SD.SessionCart)!= null)
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));
                }
                else
                {
                    var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count;
                    HttpContext.Session.SetInt32(SD.SessionCart, count);
                    return View(HttpContext.Session.GetInt32(SD.SessionCart));

                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
