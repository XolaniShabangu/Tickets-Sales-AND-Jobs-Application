using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TheBookBuffetSem2.Models;

namespace TheBookBuffetSem2
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            CreateAdminUserAndRoles();
        }

        private void CreateAdminUserAndRoles()
        {
            var context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Create Admin Role if it doesn't exist
            if (!roleManager.RoleExists("Admin"))
            {
                var role = new IdentityRole { Name = "Admin" };
                roleManager.Create(role);
            }

            // Create Customer Role if it doesn't exist
            if (!roleManager.RoleExists("Customer"))
            {
                var role = new IdentityRole { Name = "Customer" };
                roleManager.Create(role);
            }

            // Create Staff Role if it doesn't exist
            if (!roleManager.RoleExists("Staff"))
            {
                var role = new IdentityRole { Name = "Staff" };
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Recruiter"))
            {
                var role = new IdentityRole { Name = "Recruiter" };
                roleManager.Create(role);
            }


            // Create Admin User
            var adminEmail = "admin@thebookbuffet.com";
            var adminUser = userManager.FindByEmail(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };
                userManager.Create(adminUser, "Admin@12345"); // Set your admin password here
                userManager.AddToRole(adminUser.Id, "Admin");
            }

            // Create Staff User
            var staffEmail = "staff@thebookbuffet.com";
            var staffUser = userManager.FindByEmail(staffEmail);
            if (staffUser == null)
            {
                staffUser = new ApplicationUser
                {
                    UserName = staffEmail,
                    Email = staffEmail
                };
                userManager.Create(staffUser, "Staff@12345"); // Set your staff password here
                userManager.AddToRole(staffUser.Id, "Staff");
            }

            var recruiterEmail = "recruiter@thebookbuffet.com";
            var recruiterUser = userManager.FindByEmail(recruiterEmail);
            if (recruiterUser == null)
            {
                recruiterUser = new ApplicationUser
                {
                    UserName = recruiterEmail,
                    Email = recruiterEmail
                };
                userManager.Create(recruiterUser, "Recruiter@12345"); // Set your recruiter password here
                userManager.AddToRole(recruiterUser.Id, "Recruiter");
            }

        }
    }
}
