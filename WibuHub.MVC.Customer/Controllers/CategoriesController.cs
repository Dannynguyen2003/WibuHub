using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.DataLayer;
using WibuHub.MVC.Customer.ViewModels;

namespace WibuHub.MVC.Customer.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly StoryDbContext _context;

        public CategoriesController(StoryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(Guid? id)
        {
            // Mình thêm Include StoryCategories và Category vào đây để phòng trường hợp 
            // View của bạn cần hiển thị danh sách thể loại của từng truyện ra màn hình.
            var storiesQuery = _context.Stories
                .AsNoTracking()
                .Include(s => s.Chapters)
                .Include(s => s.StoryCategories)
                .ThenInclude(sc => sc.Category)
                .OrderByDescending(s => s.UpdateDate);

            if (id.HasValue)
            {
                var category = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id.Value);

                if (category == null)
                {
                    return NotFound();
                }

                // Đã sửa: Tìm qua bảng trung gian bằng .Any()
                var stories = await storiesQuery
                    .Where(s => s.StoryCategories.Any(sc => sc.CategoryId == id.Value))
                    .ToListAsync();

                return View(new CategoryStoriesViewModel
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Stories = stories
                });
            }

            var allStories = await storiesQuery.ToListAsync();

            return View(new CategoryStoriesViewModel
            {
                CategoryName = "Tất cả", // Đã sửa lỗi font chữ
                Stories = allStories
            });
        }
    }
}