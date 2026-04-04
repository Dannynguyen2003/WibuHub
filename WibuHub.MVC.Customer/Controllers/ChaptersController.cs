using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WibuHub.ApplicationCore.Entities;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ViewModels;

namespace WibuHub.MVC.Customer.Controllers
{
    public class ChaptersController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly UserManager<StoryUser> _userManager;
        private readonly IConfiguration _configuration;

        public ChaptersController(StoryDbContext context, UserManager<StoryUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> Read(Guid id)
        {
            var chapter = await _context.Chapters
                .AsNoTracking()
                .Include(c => c.Story)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (chapter == null)
            {
                return NotFound();
            }

            var storySlug = string.IsNullOrWhiteSpace(chapter.Story?.Slug)
                ? chapter.StoryName
                : chapter.Story!.Slug;

            return RedirectToRoute("ChapterReadSeo", new
            {
                storySlug,
                chapterNumber = chapter.ChapterNumber.ToString(CultureInfo.InvariantCulture)
            });
        }

        public async Task<IActionResult> ReadBySlug(string storySlug, double chapterNumber)
        {
            if (string.IsNullOrWhiteSpace(storySlug))
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .AsNoTracking()
                .Include(c => c.Story)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => !c.IsDeleted
                    && c.Story != null
                    && c.Story.Slug == storySlug
                    && c.ChapterNumber == chapterNumber);

            if (chapter == null)
            {
                return NotFound();
            }

            var isVipUser = false;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                isVipUser = currentUser?.IsVip ?? false;
            }

            var requestedServer = HttpContext.Request.Query["sv"].ToString();
            var activeServer = string.Equals(requestedServer, "vip", StringComparison.OrdinalIgnoreCase) && isVipUser
                ? "vip"
                : "normal";

            await RecordViewAsync(chapter);

            return View("Read", await BuildReadViewModelAsync(chapter, isVipUser, activeServer));
        }

        private async Task RecordViewAsync(Chapter chapter)
        {
            await _context.Stories
                .Where(s => s.Id == chapter.StoryId)
                .ExecuteUpdateAsync(s => s.SetProperty(story => story.ViewCount, story => story.ViewCount + 1));

            Guid? userId = null;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdValue = _userManager.GetUserId(User);
                if (!string.IsNullOrWhiteSpace(userIdValue) && Guid.TryParse(userIdValue, out var userGuid))
                {
                    userId = userGuid;
                }
            }

            _context.Histories.Add(new History
            {
                StoryId = chapter.StoryId,
                ChapterId = chapter.Id,
                UserId = userId
            });

            await _context.SaveChangesAsync();
        }

        private async Task<ChapterReadViewModel> BuildReadViewModelAsync(Chapter chapter, bool isVipUser, string activeServer)
        {
            var chapterList = await _context.Chapters
                .AsNoTracking()
                .Where(c => c.StoryId == chapter.StoryId && !c.IsDeleted)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();

            var currentIndex = chapterList.FindIndex(c => c.Id == chapter.Id);
            var previousId = currentIndex > 0 ? chapterList[currentIndex - 1].Id : (Guid?)null;
            var nextId = currentIndex >= 0 && currentIndex < chapterList.Count - 1
                ? chapterList[currentIndex + 1].Id
                : (Guid?)null;

            var isFollowed = false;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrWhiteSpace(userId) && Guid.TryParse(userId, out var userGuid))
                {
                    isFollowed = await _context.Follows
                        .AsNoTracking()
                        .AnyAsync(f => f.UserId == userGuid && f.StoryId == chapter.StoryId);
                }
            }

            return new ChapterReadViewModel
            {
                Chapter = chapter,
                Chapters = chapterList,
                PreviousChapterId = previousId,
                NextChapterId = nextId,
                IsFollowed = isFollowed,
                IsVipUser = isVipUser,
                ActiveServer = activeServer,
                NormalServerBaseUrl = _configuration["ReadingServers:NormalBaseUrl"],
                VipServerBaseUrl = _configuration["ReadingServers:VipBaseUrl"]
            };
        }
    }
}
