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
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Position);

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paged data
            var categories = await query
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

            var pagedResult = new PagedResult<CategoryVM>(categories, page, pageSize, totalCount);

            return View(pagedResult);
        }
    }
}