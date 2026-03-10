using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.ViewModels;

namespace WibuHub.MVC.Controllers
{
    [Authorize]
    public class AdminProfileController : Controller
    {
        private readonly UserManager<StoryUser> _userManager;

        public AdminProfileController(UserManager<StoryUser> userManager)
        {
            _userManager = userManager;
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
            user.Avatar = model.Avatar;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            ViewData["StatusMessage"] = "C?p nh?t h? sõ thành công.";
            return View(model);
        }
    }
}
