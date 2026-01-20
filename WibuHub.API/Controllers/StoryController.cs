using Microsoft.AspNetCore.Mvc;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoryController : ControllerBase
    {
        public readonly ILogger<StoryController> _logger;
        public IActionResult Index()
        {
            return View();
        }
    }
}
