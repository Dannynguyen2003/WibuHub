using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ViewModels;

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
            int myRating = 0;
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

                    myRating = await _context.Ratings
                        .AsNoTracking()
                        .Where(r => r.UserId == userGuid && r.StoryId == story.Id)
                        .Select(r => r.Score)
                        .FirstOrDefaultAsync();
                }
            }

            var ratingCount = await _context.Ratings
                .AsNoTracking()
                .CountAsync(r => r.StoryId == story.Id);

            var storyComments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.StoryId == story.Id && c.ParentId == null)
                .OrderByDescending(c => c.CreateDate)
                .Take(30)
                .ToListAsync();

            var commenterIds = storyComments
                .Select(c => c.UserId.ToString())
                .Distinct()
                .ToList();

            var commenters = await _userManager.Users
                .AsNoTracking()
                .Where(u => commenterIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Avatar })
                .ToDictionaryAsync(u => u.Id, u => new { u.UserName, u.Avatar });

            var commentItems = storyComments
                .Select(comment =>
                {
                    commenters.TryGetValue(comment.UserId.ToString(), out var commenter);
                    return new StoryCommentItemViewModel
                    {
                        Id = comment.Id,
                        UserName = string.IsNullOrWhiteSpace(commenter?.UserName) ? "Người dùng" : commenter.UserName,
                        Avatar = commenter?.Avatar,
                        Content = comment.Content,
                        CreateDate = comment.CreateDate
                    };
                })
                .ToList();

            ViewData["IsFollowed"] = isFollowed;
            ViewData["MyRating"] = myRating;
            ViewData["RatingCount"] = ratingCount;
            ViewData["StoryComments"] = commentItems;
            return View(story);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Guid storyId, string content, string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["CommentMessage"] = "Bạn phải đăng nhập để bình luận truyện.";
                return RedirectToAction(nameof(Details), new { id = storyId });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["CommentMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction(nameof(Details), new { id = storyId });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                TempData["CommentMessage"] = "Không xác định được người dùng.";
                return RedirectToAction(nameof(Details), new { id = storyId });
            }

            _context.Comments.Add(new Comment
            {
                Id = Guid.NewGuid(),
                StoryId = storyId,
                UserId = userGuid,
                Content = content.Trim(),
                CreateDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["CommentMessage"] = "Bình luận của bạn đã được đăng.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = storyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(Guid storyId, int score, string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["RatingMessage"] = "Bạn phải đăng nhập để đánh giá truyện.";
                return RedirectToAction(nameof(Details), new { id = storyId });
            }

            if (score < 1 || score > 5)
            {
                TempData["RatingMessage"] = "Điểm đánh giá không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = storyId });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                TempData["RatingMessage"] = "Không xác định được người dùng.";
                return RedirectToAction(nameof(Details), new { id = storyId });
            }

            var existedRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.StoryId == storyId && r.UserId == userGuid);

            if (existedRating == null)
            {
                _context.Ratings.Add(new Rating
                {
                    Id = Guid.NewGuid(),
                    StoryId = storyId,
                    UserId = userGuid,
                    Score = score,
                    CreateDate = DateTime.UtcNow
                });
            }
            else
            {
                existedRating.Score = score;
                existedRating.CreateDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var avgScore = await _context.Ratings
                .Where(r => r.StoryId == storyId)
                .AverageAsync(r => (double?)r.Score) ?? 0;

            await _context.Stories
                .Where(s => s.Id == storyId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.RatingScore, avgScore));

            TempData["RatingMessage"] = "Đánh giá của bạn đã được ghi nhận.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = storyId });
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
