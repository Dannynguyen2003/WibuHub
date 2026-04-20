using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ViewModels;

namespace WibuHub.MVC.Customer.Controllers
{
    public class RankingsController : Controller
    {
        private readonly StoryDbContext _context;

        public RankingsController(StoryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? type)
        {
            var normalized = (type ?? "top-day").ToLowerInvariant();
            IQueryable<ApplicationCore.Entities.Story> query = _context.Stories
                .AsNoTracking()
                .Include(s => s.Chapters);
            var title = "Top Ngày";

            switch (normalized)
            {
                case "top-week":
                    title = "Top Week";
                    query = query.OrderByDescending(s => s.ViewCount);
                    break;
                case "top-month":
                    title = "Top Month";
                    query = query.OrderByDescending(s => s.ViewCount);
                    break;
                case "favorites":
                    title = "Favorites";
                    query = query.OrderByDescending(s => s.FollowCount);
                    break;
                case "latest":
                    title = "Latest Story";
                    query = query.OrderByDescending(s => s.UpdateDate);
                    break;
                case "new":
                    title = "New Story";
                    query = query.OrderByDescending(s => s.CreatedAt);
                    break;
                case "full":
                    title = "Full Story";
                    query = query.Where(s => s.Status == 1).OrderByDescending(s => s.UpdateDate);
                    break;
                case "random":
                    var randomStoryId = await _context.Stories
                        .AsNoTracking()
                        .OrderBy(s => Guid.NewGuid())
                        .Select(s => s.Id)
                        .FirstOrDefaultAsync();

                    if (randomStoryId == Guid.Empty)
                    {
                        return RedirectToAction(nameof(Index), new { type = "top-day" });
                    }

                    return RedirectToAction("Details", "Stories", new { id = randomStoryId });
            }

            var stories = await query.Take(30).ToListAsync();

            return View(new RankingStoriesViewModel
            {
                Title = title,
                Stories = stories
            });
        }
    }
}
