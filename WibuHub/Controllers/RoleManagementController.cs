using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;
using WibuHub.MVC.ViewModels;

namespace WibuHub.MVC.Controllers
{
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public class RoleManagementController : Controller
    {
        private readonly UserManager<StoryUser> _userManager;
        private readonly RoleManager<StoryRole> _roleManager;

        public RoleManagementController(UserManager<StoryUser> userManager, RoleManager<StoryRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var model = new List<UserRoleListVM>();
            foreach (var user in users)
            {
                var roles = (await _userManager.GetRolesAsync(user)).ToList();
                if (roles.Count == 0)
                {
                    roles.Add(AppConstants.Roles.Customer);
                }

                model.Add(new UserRoleListVM
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? string.Empty,
                    Roles = roles
                });
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => r.Name!)
                .ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserRoleEditVM
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? string.Empty,
                AllRoles = roles,
                SelectedRoles = userRoles.ToList(),
                IsSuperAdmin = string.Equals(user.Email, AppConstants.SuperAdminEmail, StringComparison.OrdinalIgnoreCase)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserRoleEditVM model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.SelectedRoles?.ToList() ?? new List<string>();

            if (string.Equals(user.Email, AppConstants.SuperAdminEmail, StringComparison.OrdinalIgnoreCase)
                && !selectedRoles.Contains(AppConstants.Roles.SuperAdmin))
            {
                selectedRoles.Add(AppConstants.Roles.SuperAdmin);
            }

            var rolesToAdd = selectedRoles.Except(userRoles).ToList();
            var rolesToRemove = userRoles.Except(selectedRoles).ToList();

            if (rolesToAdd.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            if (rolesToRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                model.AllRoles = await _roleManager.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => r.Name!)
                    .ToListAsync();
                model.SelectedRoles = selectedRoles;
                model.Email = user.Email ?? user.UserName ?? string.Empty;
                model.IsSuperAdmin = string.Equals(user.Email, AppConstants.SuperAdminEmail, StringComparison.OrdinalIgnoreCase);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
