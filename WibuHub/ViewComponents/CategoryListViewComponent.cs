using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Thêm lọc IsDeleted để đồng bộ với logic Soft Delete của bạn
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Position)
                .Select(c => new CategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug, // BỔ SUNG TRƯỜNG NÀY
                    Description = c.Description,
                    Position = c.Position
                })
                .ToListAsync();

            return View(categories);
        }
    }
}