using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;

namespace WibuHub.MVC.ViewComponents
{
    public class CategoryList : ViewComponent
    {
        private readonly StoryDbContext _context;

        public CategoryList(StoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IViewComponentResult> InvokeAsync(int page = 1, int pageSize = 10)
        {
            // Thêm lọc IsDeleted để đồng bộ với logic Soft Delete của bạn
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Categories
                .Where(c => !c.IsDeleted);

            var totalCount = await query.LongCountAsync();

            var categories = await query
                .OrderBy(c => c.Position)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    Position = c.Position
                })
                .ToListAsync();

            var result = new PagedResult<CategoryVM>(categories, page, pageSize, totalCount);

            return View(result);
        }
    }
}