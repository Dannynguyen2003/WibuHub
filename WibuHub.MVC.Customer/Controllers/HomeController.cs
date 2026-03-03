using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.Models;

namespace WibuHub.MVC.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StoryDbContext _context;

        public HomeController(ILogger<HomeController> logger, StoryDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var chapters = new List<WibuHub.ApplicationCore.Entities.Chapter>();
            try
            {
                chapters = await _context.Chapters
                    .Include(c => c.Story)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(12)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tải danh sách chapter");
            }
            return View(chapters);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
