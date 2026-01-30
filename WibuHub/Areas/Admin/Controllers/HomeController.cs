using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.DataLayer;

namespace WibuHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly StoryDbContext _context;

        public HomeController(StoryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalStories = await _context.Stories.CountAsync(s => !s.IsDeleted);
            ViewBag.TotalChapters = await _context.Chapters.CountAsync(c => !c.IsDeleted);
            ViewBag.TotalAuthors = await _context.Authors.CountAsync(a => !a.IsDeleted);
            ViewBag.TotalCategories = await _context.Categories.CountAsync(c => !c.IsDeleted);
            ViewBag.TotalComments = await _context.Comments.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            return View();
        }
    }
}
