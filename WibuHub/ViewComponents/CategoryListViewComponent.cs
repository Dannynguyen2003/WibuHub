using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.DataLayer;
using WibuHub.ApplicationCore.DTOs.Shared;

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
            var categories = await _context.Categories
                .OrderBy(c => c.Position)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Position = c.Position
                })
                .ToListAsync();
            return View(categories);
        }
    }
}
