using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualBasic;
using Net6.DataAccess.Repository.IRepository;
using Net6.Models;
using Net6.Models.ViewModels;
using Net6.Utility;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Net6Core.Areas.Identity
{
    public class Register2Model : PageModel
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork unitOfWork;

        [BindProperty]
        public Register Model { get; set; }

        public Register2Model(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
                        IUserStore<IdentityUser> userStore,IEmailSender emailSender, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            this.unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }
        public void OnGet()
        {

            Model = new Register()
            {
                RoleList = roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                }),
                CompanyList = unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = Model.Email,
                    Email = Model.Email,
                    Name = Model.Name,
                    StreetAddress = Model.StreetAddress,
                    City = Model.City,
                    PostalCode = Model.PostalCode,
                    PhoneNumber = Model.PhoneNumber,
                    State = Model.State,
                    CompanyId = (Model.Role == SD.Role_User_Comp) ? Model.CompanyId : null,
                };
                var result = await userManager.CreateAsync(user, Model.Password);
                //await _emailStore.SetEmailAsync(user, Model.Email, CancellationToken.None);

                if (result.Succeeded)
                {

                    var userId = await userManager.GetUserIdAsync(user);
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code},
                        protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Model.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (Model.Role == null)
                    {
                        await userManager.AddToRoleAsync(user, SD.Role_User_Indi);
                    }
                    else
                    {
                        await userManager.AddToRoleAsync(user, Model.Role);
                    }

                    if(User.IsInRole(SD.Role_Admin)){
                        TempData["success"] = "New User Created Successfully";
                    }
                    else
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToPage("Index", "Home", new { area = "Customer" });
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            Model.RoleList = roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });
            Model.CompanyList = unitOfWork.Company.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            return Page();
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
