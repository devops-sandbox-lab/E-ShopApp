using Microsoft.AspNetCore.Identity;
using Eshop.Core.Entities;
using Eshop.Core.Enums;

namespace Eshop.Application.Helpers
{
    public static class AdminInitializer
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@Eshop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail.ToLower(),
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    SecurityQuestion = "admin",
                    Role = UserRole.Admin,


                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
                }
            }
        }
    }
}
