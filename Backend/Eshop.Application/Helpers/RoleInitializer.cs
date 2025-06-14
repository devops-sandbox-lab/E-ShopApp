using Microsoft.AspNetCore.Identity;
using Eshop.Core.Enums;

namespace Application.Helpers
{
    public static class RoleInitializer
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (UserRole roleEnumValue in Enum.GetValues(typeof(UserRole)))
            {
                string roleName = roleEnumValue.ToString();
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}