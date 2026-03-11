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
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-6);

            var dailyViews = await _dbContext.Histories
                .Where(h => h.ReadTime >= startDate && h.ReadTime < endDate.AddDays(1))
                .GroupBy(h => h.ReadTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var labels = new List<string>(7);
            var counts = new List<int>(7);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                labels.Add(GetVietnameseDayLabel(date.DayOfWeek));
                counts.Add(dailyViews.FirstOrDefault(x => x.Date == date)?.Count ?? 0);
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
                    Title = $"B?nh lu?n m?i: {(c.Content.Length > 60 ? c.Content.Substring(0, 60) + "..." : c.Content)}",
                    OccurredAt = c.CreateDate
                })
                .Take(3)
                .ToListAsync();

            var recentOrders = await _dbContext.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new DashboardActivityItem
                {
                    IconClass = "fas fa-receipt",
                    Title = $"Ðõn hàng m?i #{o.Id.ToString().Substring(0, 8)}",
                    OccurredAt = o.CreatedAt
                })
                .Take(3)
                .ToListAsync();

            var recentActivities = recentChapters
                .Concat(recentComments)
                .Concat(recentOrders)
                .OrderByDescending(a => a.OccurredAt)
                .Take(5)
                .ToList();

            var recentStories = await _dbContext.Stories
                .OrderByDescending(s => s.UpdateDate)
                .Select(s => new DashboardStoryItem
                {
                    Id = s.Id,
                    StoryName = s.StoryName,
                    AuthorName = s.Author != null ? s.Author.Name : s.AuthorName,
                    ChapterCount = s.Chapters.Count,
                    ViewCount = s.ViewCount,
                    Status = s.Status
                })
                .Take(5)
                .ToListAsync();

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
