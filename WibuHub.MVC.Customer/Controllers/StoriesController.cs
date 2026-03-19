using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
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
                .Include(s => s.StoryCategories).ThenInclude(sc => sc.Category)
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
        [HttpGet]
        public async Task<IActionResult> Read(Guid storyId, double chapter)
        {
            // 1. Lấy chapter hiện tại
            var currentChapter = await _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Images) // hoặc ChapterImages tùy tên bảng của bạn
                .FirstOrDefaultAsync(c => c.StoryId == storyId && c.ChapterNumber == chapter);

            if (currentChapter == null) return NotFound();

            // 2. Lấy danh sách tất cả chapter của truyện này (để đổ vào cái Select/Dropdown)
            var allChapters = await _context.Chapters
                .Where(c => c.StoryId == storyId)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();

            // 3. Tính toán Chapter trước và sau
            var prevId = allChapters.LastOrDefault(c => c.ChapterNumber < chapter)?.Id;
            var nextId = allChapters.FirstOrDefault(c => c.ChapterNumber > chapter)?.Id;

            // 4. Kiểm tra xem user đã theo dõi chưa (để hiện nút Tim đỏ/trắng)
            var isFollowed = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = Guid.Parse(_userManager.GetUserId(User));
                isFollowed = await _context.Follows.AnyAsync(f => f.UserId == userId && f.StoryId == storyId);
            }

            // 5. Đóng gói vào ViewModel mà bạn đã định nghĩa
            var viewModel = new WibuHub.MVC.Customer.ViewModels.ChapterReadViewModel
            {
                Chapter = currentChapter,
                Chapters = allChapters,
                PreviousChapterId = prevId,
                NextChapterId = nextId,
                IsFollowed = isFollowed
            };

            return View(viewModel); // Trả về View sử dụng ViewModel mới
        }
        public async Task<IActionResult> FilterByCategory(Guid categoryId)
        {
            // Lấy tên thể loại để hiện tiêu đề trang
            var category = await _context.Categories.FindAsync(categoryId);
            ViewData["CategoryName"] = category?.Name;

            var stories = await _context.Stories
                .AsNoTracking()
                // QUAN TRỌNG: Kiểm tra xem trong danh sách thể loại của truyện có chứa ID này không
                .Where(s => s.StoryCategories.Any(sc => sc.CategoryId == categoryId))
                .Include(s => s.Author)
                .Include(s => s.StoryCategories)
                    .ThenInclude(sc => sc.Category)
                .ToListAsync();

            return View("Index", stories); // Trả về view danh sách truyện
        }
    }
}
