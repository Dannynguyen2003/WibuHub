using Microsoft.AspNetCore.Identity;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;
namespace WibuHub.ApplicationCore.Configuration
{
    public class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<StoryRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(AppConstants.Roles.SuperAdmin))
            {
                var superAdminRole = new StoryRole
                {
                    Name = AppConstants.Roles.SuperAdmin,
                    Description = "Quản trị cao nhất, có toàn quyền hệ thống"
                };
                await roleManager.CreateAsync(superAdminRole);
            }

            // Seed Admin role
            if (!await roleManager.RoleExistsAsync(AppConstants.Roles.Admin))
            {
                var adminRole = new StoryRole
                {
                    Name = AppConstants.Roles.Admin,
                    Description = "Người vận hành hệ thống, quản lý truyện, chapter và người dùng"
                };
                await roleManager.CreateAsync(adminRole);
            }

            if (!await roleManager.RoleExistsAsync(AppConstants.Roles.Uploader))
            {
                var uploaderRole = new StoryRole
                {
                    Name = AppConstants.Roles.Uploader,
                    Description = "Người upload truyện và chapter"
                };
                await roleManager.CreateAsync(uploaderRole);
            }
        }
    }
}