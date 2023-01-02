using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net6.DataAccess.Repository.IRepository;
using Net6.Models;
using Net6.Models.ViewModels;
using Net6.Utility;
using Net6Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net6.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _db = db;  
        }
        public void Initialize()
        {
            //migration if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count()>0){
                    _db.Database.Migrate();
                }
            }catch(Exception ex)
            {

            }
            //creat roles if  they are not created
            if (!roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();

                //if roles are not created, then we will create admin user as well

                userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@dotnet.com",
                    Email = "admin@dotnet.com",
                    Name = "Brad Pit",
                    PhoneNumber = "111222233",
                    StreetAddress = "New York 123 Ave",
                    State = "US",
                    PostalCode = "1235647",
                    City = "Chicago",
                }, "Garcia82_").GetAwaiter().GetResult();
                ApplicationUser user= _db.ApplicationUsers.FirstOrDefault(q=>q.Email == "admin@dotnet.com");
                userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                
            }
            return;
        }
    }
}
