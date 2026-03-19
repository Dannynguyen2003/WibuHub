using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;

namespace WibuHub.MVC.ViewComponents
{
    public class StoryList : ViewComponent
    {
        private readonly StoryDbContext _context;

        public StoryList(StoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IViewComponentResult> InvokeAsync(int page = 1, int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Stories
                .Where(s => !s.IsDeleted)
                .Include(s => s.Author)
                .Include(s => s.StoryCategories)
                .ThenInclude(sc => sc.Category);

            var totalCount = await query.LongCountAsync();

            var stories = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to ViewModel
            var storyVMs = stories.Select(s =>
            {
                // Lấy ra danh sách tên thể loại từ bảng trung gian StoryCategories
                var categoryNames = s.StoryCategories
                    .Select(sc => sc.Category?.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct()
                    .ToList();

                return new StoryVM
                {
                    Id = s.Id,
                    Title = s.StoryName,
                    AlternativeName = s.AlternativeName,
                    Description = s.Description,
                    Slug = s.Slug,
                    Price = s.Price,
                    Discount = s.Discount,
                    Status = s.Status,
                    ViewCount = s.ViewCount,
                    FollowCount = s.FollowCount,
                    RatingScore = s.RatingScore,
                    CreatedAt = s.CreatedAt,
                    UpdateDate = s.UpdateDate,
                    CoverImage = s.CoverImage,
                    AuthorId = s.AuthorId,
                    Author = s.Author,
                    // Hiển thị các thể loại cách nhau bằng dấu phẩy
                    CategoryDisplay = categoryNames.Any() ? string.Join(", ", categoryNames) : string.Empty
                };
            }).ToList();

            var result = new PagedResult<StoryVM>(storyVMs, page, pageSize, totalCount);
            return View(result);
        }
    }
}