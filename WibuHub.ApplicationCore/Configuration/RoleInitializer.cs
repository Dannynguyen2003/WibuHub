using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;

namespace WibuHub.ApplicationCore.Configuration
{
    public static class RoleInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<StoryRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<StoryUser>>();

            // Define all roles with descriptions
            var roles = new Dictionary<string, string>
            {
                { AppConstants.RoleSuperAdmin, "Super Administrator with full system access" },
                { AppConstants.RoleAdmin, "General Administrator" },
                { AppConstants.RoleContentManager, "Manages content: upload and translate stories" },
                { AppConstants.RoleStoryManager, "Manages stories and chapters" },
                { AppConstants.RoleSalesManager, "Manages sales and orders" },
                { AppConstants.RoleCustomer, "Regular customer who reads and purchases stories" }
            };

            // Create roles if they don't exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Key))
                {
                    var storyRole = new StoryRole
                    {
                        Name = role.Key,
                        Description = role.Value
                    };
                    await roleManager.CreateAsync(storyRole);
                }
            }

            // Create default SuperAdmin user if it doesn't exist
            var superAdminEmail = "superadmin@wibuhub.com";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdminUser == null)
            {
                superAdminUser = new StoryUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    FullName = "Super Administrator",
                    UserType = AppConstants.UserTypeAdmin,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, AppConstants.RoleSuperAdmin);
                }
            }
            else
            {
                // Ensure existing SuperAdmin has the SuperAdmin role
                if (!await userManager.IsInRoleAsync(superAdminUser, AppConstants.RoleSuperAdmin))
                {
                    await userManager.AddToRoleAsync(superAdminUser, AppConstants.RoleSuperAdmin);
                }
            }
        }
    }
}
