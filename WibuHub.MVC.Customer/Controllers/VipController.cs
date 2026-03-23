using Microsoft.AspNetCore.Mvc;

namespace WibuHub.MVC.Customer.Controllers
{
    // Controller này làm nhiệm vụ duy nhất là trả về mã HTML giao diện cho trình duyệt
    public class VipController : Controller
    {
        [HttpGet]
        [Route("vip/upgrade")]
        public IActionResult Upgrade()
        {
            return View();
        }
    }
}