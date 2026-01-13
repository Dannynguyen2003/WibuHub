using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities.Identity;

namespace WibuHub.MVC.Admin.Controllers
{
    [Route("api/admin/customers")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CustomerController : ControllerBase
    {
        private readonly UserManager<StoryUser> _userManager;
        private readonly RoleManager<StoryRole> _roleManager; // <--- THÊM CÁI NÀY

        public CustomerController(
            UserManager<StoryUser> userManager,
            RoleManager<StoryRole> roleManager) // <--- Inject vào đây
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ... Các hàm GetUsers, LockUser giữ nguyên ...

        // ==========================================================
        // QUẢN LÝ ROLE CỦA USER (Assign Role)
        // ==========================================================

        /// <summary>
        /// Lấy tất cả các Role đang có trong hệ thống (để hiện dropdown chọn)
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles
                .Select(r => new { r.Id, r.Name, r.Description }) // Giả sử StoryRole có Description
                .ToListAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Cấp quyền cho User (Ví dụ: Thăng chức làm Uploader)
        /// </summary>
        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AddRoleToUser(string userId, [FromBody] UpdateRoleDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User không tồn tại");

            // 1. Kiểm tra Role có tồn tại trong hệ thống không
            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists) return BadRequest($"Role '{request.RoleName}' không tồn tại.");

            // 2. Thêm Role cho User
            var result = await _userManager.AddToRoleAsync(user, request.RoleName);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok($"Đã thêm quyền {request.RoleName} cho user {user.UserName}.");
        }

        /// <summary>
        /// Gỡ quyền của User (Ví dụ: Hạ chức)
        /// </summary>
        [HttpDelete("{userId}/roles")]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, [FromBody] UpdateRoleDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok($"Đã gỡ quyền {request.RoleName} khỏi user.");
        }
    }
}