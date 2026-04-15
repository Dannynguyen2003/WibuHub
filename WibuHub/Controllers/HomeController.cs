using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WibuHub.DataLayer;
using WibuHub.MVC.Models;
using WibuHub.ViewModels;

namespace WibuHub.MVC.Controllers
{
    //[Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StoryDbContext _dbContext;
        private readonly StoryIdentityDbContext _identityDbContext;

        public HomeController(ILogger<HomeController> logger, StoryDbContext dbContext, StoryIdentityDbContext identityDbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _identityDbContext = identityDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-6);

            var dailyViews = await _dbContext.Histories
                .Where(h => h.ReadTime >= startDate && h.ReadTime < endDate.AddDays(1))
                .GroupBy(h => h.ReadTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var dailyViewLookup = dailyViews.ToDictionary(
                x => DateOnly.FromDateTime(x.Date),
                x => x.Count);

            var labels = new List<string>(7);
            var counts = new List<int>(7);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                labels.Add(GetVietnameseDayLabel(date.DayOfWeek));
                counts.Add(dailyViewLookup.GetValueOrDefault(DateOnly.FromDateTime(date), 0));
            }

            var recentChapters = await _dbContext.Chapters
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new DashboardActivityItem
                {
                    IconClass = "fas fa-book",
                    Title = $"{c.StoryName} chap {c.ChapterNumber}",
                    OccurredAt = c.CreatedAt
                })
                .Take(3)
                .ToListAsync();

            var recentComments = await _dbContext.Comments
                .OrderByDescending(c => c.CreateDate)
                .Select(c => new DashboardActivityItem
                {
                    IconClass = "fas fa-comment",
                    Title = $"Bình luận mới: {(c.Content.Length > 60 ? c.Content.Substring(0, 60) + "..." : c.Content)}",
                    OccurredAt = c.CreateDate
                })
                .Take(3)
                .ToListAsync();

            var recentCategories = await _dbContext.Categories
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new DashboardActivityItem
                {
                    IconClass = "fas fa-tags",
                    Title = $"Danh mục mới: {c.Name}",
                    OccurredAt = c.CreatedAt
                })
                .Take(3)
                .ToListAsync();

            var recentOrders = await _dbContext.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new DashboardActivityItem
                {
                    IconClass = "fas fa-receipt",
                    Title = $"Đơn hàng mới #{o.Id.ToString().Substring(0, 8)}",
                    OccurredAt = o.CreatedAt
                })
                .Take(3)
                .ToListAsync();

            var recentStoryRaw = await _dbContext.Stories
                .OrderByDescending(s => s.UpdateDate)
                .Select(s => new
                {
                    s.StoryName,
                    AuthorName = s.Author != null ? s.Author.Name : s.AuthorName,
                    s.LatestChapter,
                    Categories = s.StoryCategories
                        .Select(sc => sc.Category.Name)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Take(3)
                        .ToList(),
                    s.UpdateDate
                })
                .Take(5)
                .ToListAsync();

            var recentStoryActivities = recentStoryRaw
                .Select(s => new DashboardActivityItem
                {
                    IconClass = "fas fa-tags",
                    Title = $"Truyện: {s.StoryName} | Tác giả: {(string.IsNullOrWhiteSpace(s.AuthorName) ? "Đang cập nhật" : s.AuthorName)} | Thể loại: {(s.Categories.Count > 0 ? string.Join(", ", s.Categories) : "N/A")} | Chapter: {(string.IsNullOrWhiteSpace(s.LatestChapter) ? "Đang cập nhật" : s.LatestChapter)}",
                    OccurredAt = s.UpdateDate
                })
                .ToList();

            var recentActivities = recentChapters
                .Concat(recentComments)
                .Concat(recentCategories)
                .Concat(recentOrders)
                // 2. UPDATE THE CONCAT CALL to use the new variable name
                .Concat(recentStoryActivities)
                .OrderByDescending(a => a.OccurredAt)
                .Take(5)
                .ToList();

            var recentStorySeed = await _dbContext.Stories
                .OrderByDescending(s => s.UpdateDate)
                .Select(s => new DashboardStoryItem
                {
                    Id = s.Id,
                    StoryName = s.StoryName,
                    AuthorName = s.Author != null ? s.Author.Name : s.AuthorName,
                    ChapterCount = s.TotalChapters,
                    ViewCount = s.ViewCount,
                    Status = s.Status
                })
                .Take(5)
                .ToListAsync();

            var recentStoryIds = recentStorySeed.Select(s => s.Id).ToList();

            var chapterCountLookup = await _dbContext.Chapters
                .Where(c => recentStoryIds.Contains(c.StoryId) && !c.IsDeleted)
                .GroupBy(c => c.StoryId)
                .Select(g => new { StoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StoryId, x => x.Count);

            var historyViewLookup = await _dbContext.Histories
                .Where(h => recentStoryIds.Contains(h.StoryId))
                .GroupBy(h => h.StoryId)
                .Select(g => new { StoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StoryId, x => (long)x.Count);

            var recentStories = recentStorySeed
                .Select(story =>
                {
                    story.ChapterCount = chapterCountLookup.GetValueOrDefault(story.Id, story.ChapterCount);
                    if (story.ViewCount <= 0)
                    {
                        story.ViewCount = historyViewLookup.GetValueOrDefault(story.Id, 0);
                    }

                    return story;
                })
                .ToList();

            var paidStatuses = new[] { "paid", "success", "completed", "successful" };

            var model = new AdminDashboardVM
            {
                TotalStories = await _dbContext.Stories.CountAsync(),
                TotalChapters = await _dbContext.Chapters.CountAsync(),
                TotalUsers = await _identityDbContext.StoryUsers.CountAsync(),
                TotalViews = await _dbContext.Stories.SumAsync(s => (long?)s.ViewCount) ?? 0,
                TotalRevenue = await _dbContext.Orders
                    .Where(o => o.PaymentStatus != null && paidStatuses.Contains(o.PaymentStatus.ToLower()))
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                TotalOrders = await _dbContext.Orders.CountAsync(),
                ViewLabels = labels,
                ViewCounts = counts,
                RecentActivities = recentActivities,
                RecentStories = recentStories
            };

            return View(model);
        }

        private static string GetVietnameseDayLabel(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "CN",
                DayOfWeek.Monday => "T2",
                DayOfWeek.Tuesday => "T3",
                DayOfWeek.Wednesday => "T4",
                DayOfWeek.Thursday => "T5",
                DayOfWeek.Friday => "T6",
                DayOfWeek.Saturday => "T7",
                _ => string.Empty
            };
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
