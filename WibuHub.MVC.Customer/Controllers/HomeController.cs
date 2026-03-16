using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ViewModels; // Đảm bảo namespace này khớp với project của bạn

namespace WibuHub.MVC.Customer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StoryDbContext _context;

        // Đã sửa: Phải truyền StoryDbContext vào constructor
        public HomeController(ILogger<HomeController> logger, StoryDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy truyện có lượt xem cao nhất
            var featuredStory = await _context.Stories
                .OrderByDescending(s => s.ViewCount)
                .FirstOrDefaultAsync();

            // Nếu bạn muốn hiển thị cả Slider Truyện Mới ngoài view Index, 
            // bạn có thể nhét nó vào ViewBag hoặc dùng 1 HomeViewModel tổng hợp.
            // Ví dụ dùng ViewBag:
            ViewBag.NewStories = await GetNewlyUploadedStoriesAsync();

            return View(featuredStory);
        }

        public async Task<List<StoryViewModel>> GetNewlyUploadedStoriesAsync()
        {
            // Bước 1: Lấy dữ liệu từ Database lên (Chỉ dùng các phép toán EF Core hỗ trợ)
            var rawStories = await _context.Stories
                // SỬA LỖI 1: Thay "Published" bằng con số đại diện cho trạng thái đó trong DB. 
                // Ví dụ: 1 là Published, 0 là Draft. Bạn hãy thay số 1 bằng số đúng của bạn nhé!
                .Where(s => s.Status == 1)
                .OrderByDescending(s => s.CreatedAt)
                .Take(12)
                .Select(s => new
                {
                    Id = s.Id,
                    StoryName = s.StoryName,
                    CoverImage = s.CoverImage,
                    LatestChapter = s.Chapters.OrderByDescending(c => c.CreatedAt).FirstOrDefault().Name,
                    CreatedAt = s.CreatedAt // Kéo ngày tạo ra trước
                })
                .ToListAsync();

            // Bước 2: Map dữ liệu sang ViewModel và xử lý hàm CalculateTimeAgo trên RAM (C#)
            var viewModels = rawStories.Select(s => new StoryViewModel
            {
                Id = s.Id,
                StoryName = s.StoryName,
                CoverImage = s.CoverImage,
                LatestChapter = s.LatestChapter,
                // SỬA LỖI 2: Gọi hàm C# ở đây thì EF Core sẽ không báo lỗi
                TimeAgo = CalculateTimeAgo(s.CreatedAt)
            }).ToList();

            return viewModels;
        }

        // Đã bổ sung: Hàm tính toán thời gian trôi qua (vd: "4 Giờ trước")
        private static string CalculateTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.Now - createdAt;

            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} Phút trước";

            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} Giờ trước";

            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} Ngày trước";

            // Nếu quá 30 ngày thì hiển thị ngày tháng năm
            return createdAt.ToString("dd/MM/yyyy");
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