using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;

namespace WibuHub.MVC.Admin.Controllers
{
    [Route("api/admin/roles")]
    [ApiController]
    [Authorize(Roles = AppConstants.Roles.Admin)] // Only Admin can manage roles
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<StoryRole> _roleManager;

        public RoleController(RoleManager<StoryRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: api/admin/roles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        // POST: api/admin/roles
        // Tạo Role mới (VD: "ContentCreator")
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return BadRequest("Tên quyền không được để trống");

            // StoryRole thường có constructor nhận Name, hoặc bạn new StoryRole { Name = ... }
            var newRole = new StoryRole { Name = roleName, NormalizedName = roleName.ToUpper() };

            var result = await _roleManager.CreateAsync(newRole);

            if (result.Succeeded) return Ok($"Đã tạo role: {roleName}");

            return BadRequest(result.Errors);
        }

        // DELETE: api/admin/roles/Moderator
        [HttpDelete("{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound("Role không tìm thấy");

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded) return Ok($"Đã xóa role: {roleName}");

            return BadRequest(result.Errors);
        }
    }
}

