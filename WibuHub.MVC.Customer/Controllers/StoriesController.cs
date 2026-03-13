using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;

namespace WibuHub.MVC.Customer.Controllers
{
    public class StoriesController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly UserManager<StoryUser> _userManager;

        public StoriesController(StoryDbContext context, UserManager<StoryUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var story = await _context.Stories
                .AsNoTracking()
                .Include(s => s.Author)
                .Include(s => s.Category)
                .Include(s => s.StoryCategories)
                .ThenInclude(sc => sc.Category)
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (story == null)
            {
                return NotFound();
            }

            var isFollowed = false;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrWhiteSpace(userId) && Guid.TryParse(userId, out var userGuid))
                {
                    isFollowed = await _context.Follows
                        .AsNoTracking()
                        .AnyAsync(f => f.UserId == userGuid && f.StoryId == story.Id);

                    var continueChapterNumber = await _context.Histories
                        .AsNoTracking()
                        .Where(h => h.UserId == userGuid && h.StoryId == story.Id)
                        .OrderByDescending(h => h.ReadTime)
                        .Select(h => (double?)h.Chapter.ChapterNumber)
                        .FirstOrDefaultAsync();

                    ViewData["ContinueChapterNumber"] = continueChapterNumber;
                }
            }

            ViewData["IsFollowed"] = isFollowed;
            return View(story);
        }
    }
}
