using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.ViewModels;

namespace WibuHub.MVC.Controllers
{
    [Authorize]
    public class AdminProfileController : Controller
    {
        private readonly UserManager<StoryUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdminProfileController(UserManager<StoryUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var model = new AdminProfileVM
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                Email = user.Email,
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AdminProfileVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            model.Email = user.Email;
            model.UserName = user.UserName;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var extension = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();
                var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.AvatarFile), "Invalid image format.");
                    return View(model);
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{user.Id}_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using (var stream = System.IO.File.Create(filePath))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.Avatar = $"/uploads/avatars/{fileName}";
                model.Avatar = user.Avatar;
            }
            else
            {
                user.Avatar = model.Avatar;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            ViewData["StatusMessage"] = "Profile updated successfully.";
            return View(model);
        }
    }
}
