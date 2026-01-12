using Microsoft.AspNetCore.Mvc;

namespace WibuHub.MVC.Admin.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
