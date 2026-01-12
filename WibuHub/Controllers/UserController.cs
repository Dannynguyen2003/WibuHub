using Microsoft.AspNetCore.Mvc;

namespace WibuHub.MVC.Admin.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
