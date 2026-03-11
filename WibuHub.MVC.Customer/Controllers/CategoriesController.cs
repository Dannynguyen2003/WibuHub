using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ViewModels;

namespace WibuHub.MVC.Customer.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly StoryDbContext _context;

        public CategoriesController(StoryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(Guid? id)
        {
            var storiesQuery = _context.Stories
                .AsNoTracking()
                .Include(s => s.Chapters)
                .OrderByDescending(s => s.UpdateDate);

            if (id.HasValue)
            {
                var category = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id.Value);

                if (category == null)
                {
                    return NotFound();
                }

                var stories = await storiesQuery
                    .Where(s => s.CategoryId == id.Value)
                    .ToListAsync();

                return View(new CategoryStoriesViewModel
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Stories = stories
                });
            }

            var allStories = await storiesQuery.ToListAsync();

            return View(new CategoryStoriesViewModel
            {
                CategoryName = "T?t c?",
                Stories = allStories
            });
        }
    }
}
