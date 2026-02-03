using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;
using WibuHub.MVC.ViewModels;

namespace WibuHub.MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin")] // Chỉ SuperAdmin mới được quản lý roles
    public class RolesController : Controller
    {
        private readonly RoleManager<StoryRole> _roleManager;
        private readonly UserManager<StoryUser> _userManager;

        public RolesController(RoleManager<StoryRole> roleManager, UserManager<StoryUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleVMs = new List<RoleVM>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleVMs.Add(new RoleVM
                {
                    Id = role.Id,
                    Name = role.Name!,
                    Description = role.Description,
                    UserCount = usersInRole.Count
                });
            }

            return View(roleVMs);
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            var roleDetailsVM = new RoleDetailsVM
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                UserCount = usersInRole.Count,
                Users = usersInRole.Select(u => new UserInRoleVM
                {
                    UserId = u.Id,
                    Email = u.Email!,
                    FullName = u.FullName
                }).ToList()
            };

            return View(roleDetailsVM);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleVM model)
        {
            if (ModelState.IsValid)
            {
                // Check if role already exists
                var existingRole = await _roleManager.FindByNameAsync(model.Name);
                if (existingRole != null)
                {
                    ModelState.AddModelError("Name", "Role với tên này đã tồn tại");
                    return View(model);
                }

                var role = new StoryRole
                {
                    Name = model.Name,
                    Description = model.Description
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Đã tạo role '{model.Name}' thành công";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent editing of system roles
            if (role.Name == AppConstants.RoleSuperAdmin)
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa role SuperAdmin";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditRoleVM
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description
            };

            return View(model);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditRoleVM model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Prevent editing of system roles
                if (role.Name == AppConstants.RoleSuperAdmin)
                {
                    TempData["ErrorMessage"] = "Không thể chỉnh sửa role SuperAdmin";
                    return RedirectToAction(nameof(Index));
                }

                // Check if new name conflicts with existing role
                if (role.Name != model.Name)
                {
                    var existingRole = await _roleManager.FindByNameAsync(model.Name);
                    if (existingRole != null)
                    {
                        ModelState.AddModelError("Name", "Role với tên này đã tồn tại");
                        return View(model);
                    }
                    role.Name = model.Name;
                }

                role.Description = model.Description;

                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Đã cập nhật role '{model.Name}' thành công";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent deletion of system roles
            if (role.Name == AppConstants.RoleSuperAdmin || 
                role.Name == AppConstants.RoleAdmin || 
                role.Name == AppConstants.RoleCustomer)
            {
                TempData["ErrorMessage"] = "Không thể xóa role hệ thống";
                return RedirectToAction(nameof(Index));
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            var roleVM = new RoleVM
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                UserCount = usersInRole.Count
            };

            return View(roleVM);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent deletion of system roles
            if (role.Name == AppConstants.RoleSuperAdmin || 
                role.Name == AppConstants.RoleAdmin || 
                role.Name == AppConstants.RoleCustomer)
            {
                TempData["ErrorMessage"] = "Không thể xóa role hệ thống";
                return RedirectToAction(nameof(Index));
            }

            // Check if role has users
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa role vì có {usersInRole.Count} người dùng đang sử dụng";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Đã xóa role '{role.Name}' thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa role";
            return RedirectToAction(nameof(Index));
        }
    }
}
