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
                    title = "Top Tu?n";
                    query = query.OrderByDescending(s => s.ViewCount);
                    break;
                case "top-month":
                    title = "Top Tháng";
                    query = query.OrderByDescending(s => s.ViewCount);
                    break;
                case "favorites":
                    title = "Yêu Thích";
                    query = query.OrderByDescending(s => s.FollowCount);
                    break;
                case "latest":
                    title = "M?i C?p Nh?t";
                    query = query.OrderByDescending(s => s.UpdateDate);
                    break;
                case "new":
                    title = "Truy?n M?i";
                    query = query.OrderByDescending(s => s.CreatedAt);
                    break;
                case "full":
                    title = "Truy?n Full";
                    query = query.Where(s => s.Status == 1).OrderByDescending(s => s.UpdateDate);
                    break;
                case "random":
                    title = "Truy?n Ng?u Nhiên";
                    query = query.OrderBy(s => Guid.NewGuid());
                    break;
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
