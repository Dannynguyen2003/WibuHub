using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WibuHub.ApplicationCore.Entities.Identity;

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

            return View(user);
        }
    }
}
