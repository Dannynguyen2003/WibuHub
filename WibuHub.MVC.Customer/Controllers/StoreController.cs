using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.MVC.Customer.Models;

namespace WibuHub.MVC.Customer.Controllers
{
    public class StoreController : Controller
    {
        private readonly ILogger<StoreController> _logger;

        public StoreController(ILogger<StoreController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var stories = new List<StoryDto>
            {
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Title = "One Piece", AuthorName = "Eiichiro Oda" },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Title = "Naruto", AuthorName = "Masashi Kishimoto" }
            };
            return View("~/Views/Home/Index.cshtml", stories);
        }

        public IActionResult Privacy()
        {
            return View("~/Views/Home/Privacy.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
