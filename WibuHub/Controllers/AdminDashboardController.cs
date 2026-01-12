using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;
using WibuHub.MVC.Models;
using WibuHub.MVC.ViewModels;

namespace WibuHub.MVC.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AdminDashboardController : Controller
    {
        private readonly StoryDbContext _context;
        private readonly UserManager<StoryUser> _userManager;
        private readonly SignInManager<StoryUser> _signInManager;

        public AdminDashboardController(StoryDbContext context)
        {
            _context = context;
        }

        // GET: AdminDashboard
        public async Task<IActionResult> Index()
        {
            // Lấy ngày đầu tháng để tính doanh thu tháng này
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Khởi tạo ViewModel
            var model = new AdminDashboardVM();

            // 1. TỔNG HỢP SỐ LIỆU (Dùng CountAsync cho nhanh)
            // (Chạy song song các Task để tối ưu performance thay vì chờ tuần tự)
            var countStoriesTask = _context.Stories.CountAsync();
            var countUsersTask = _context.Users.CountAsync();
            var countChaptersTask = _context.Chapters.CountAsync();

            // Đếm số chương chưa publish (để Admin duyệt)
            var countPendingTask = _context.Chapters.CountAsync(c => !c.IsPublished);

            // Tính tổng tiền các đơn hàng thành công trong tháng
            var revenueTask = _context.Orders
                                .Where(o => o.CreatedDate >= startOfMonth && o.Status == "Success")
                                .SumAsync(o => o.Amount);

            // Chờ tất cả query đếm xong
            await Task.WhenAll(countStoriesTask, countUsersTask, countChaptersTask, countPendingTask, revenueTask);

            model.TotalStories = countStoriesTask.Result;
            model.TotalUsers = countUsersTask.Result;
            model.TotalChapters = countChaptersTask.Result;
            model.PendingRequests = countPendingTask.Result;
            model.MonthlyRevenue = revenueTask.Result;

            // 2. LẤY DỮ LIỆU BẢNG (Top 5 - 10)

            // Top 5 truyện view cao nhất
            model.TopViewedStories = await _context.Stories
                                            .AsNoTracking()
                                            .OrderByDescending(s => s.ViewCount)
                                            .Take(5)
                                            .ToListAsync();

            // 5 Chương vừa được upload gần nhất (kèm tên truyện để hiển thị)
            model.RecentUploadedChapters = await _context.Chapters
                                            .Include(c => c.Story) // Join sang bảng Story lấy tên truyện
                                            .AsNoTracking()
                                            .OrderByDescending(c => c.CreatedAt)
                                            .Take(5)
                                            .ToListAsync();

            // 5 Giao dịch nạp tiền mới nhất
            model.RecentOrders = await _context.Orders
                                            .Include(o => o.User) // Join lấy tên người nạp
                                            .AsNoTracking()
                                            .OrderByDescending(o => o.CreatedDate)
                                            .Take(5)
                                            .ToListAsync();

            return View(model);
        }

        // --- CÁC API PHỤC VỤ VẼ BIỂU ĐỒ (AJAX) ---

        // API trả về JSON doanh thu 7 ngày gần nhất
        // URL gọi: /AdminDashboard/GetRevenueChart
        [HttpGet]
        public async Task<IActionResult> GetRevenueChart()
        {
            var sevenDaysAgo = DateTime.Now.AddDays(-6).Date;

            var data = await _context.Orders
                .Where(o => o.CreatedDate >= sevenDaysAgo && o.Status == "Success")
                .GroupBy(o => o.CreatedDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd/MM"),
                    Revenue = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            // Đảm bảo trả về đủ 7 ngày (kể cả ngày doanh thu = 0)
            // Logic này thường xử lý ở JS frontend hoặc backend tùy bạn, ở đây trả raw data trước.
            return Json(data);
        }

        // API trả về tỷ lệ User Mới vs User Cũ (Ví dụ cho biểu đồ tròn)
        [HttpGet]
        public async Task<IActionResult> GetUserStats()
        {
            var total = await _context.StoryUser.CountAsync();
            var newUsers = await _context.Users.CountAsync(u => u.CreatedDate >= DateTime.Now.AddDays(-30));

            return Json(new
            {
                Total = total,
                NewUsersMonth = newUsers,
                Returning = total - newUsers
            });
        }
    }
}
