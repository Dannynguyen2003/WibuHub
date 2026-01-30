using Microsoft.AspNetCore.Identity;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;

namespace WibuHub.ApplicationCore.Configuration
{
    public class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<StoryRole> roleManager)
        {
            // Seed Customer role
            if (!await roleManager.RoleExistsAsync(AppConstants.Roles.Customer))
            {
                var customerRole = new StoryRole
                {
                    Name = AppConstants.Roles.Customer,
                    Description = "Người dùng đọc truyện và mua truyện"
                };
                var result = await roleManager.CreateAsync(customerRole);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create {AppConstants.Roles.Customer} role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Seed Admin role
            if (!await roleManager.RoleExistsAsync(AppConstants.Roles.Admin))
            {
                var adminRole = new StoryRole
                {
                    Name = AppConstants.Roles.Admin,
                    Description = "Người vận hành hệ thống, quản lý truyện, chapter và người dùng"
                };
                var result = await roleManager.CreateAsync(adminRole);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create {AppConstants.Roles.Admin} role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
