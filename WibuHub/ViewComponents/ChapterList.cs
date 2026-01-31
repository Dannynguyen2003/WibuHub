using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
namespace WibuHub.MVC.ViewComponents
{
    public class ChapterList : ViewComponent
    {
        private readonly StoryDbContext _context;
        public ChapterList(StoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var chapters = await _context.Chapters
                .Include(c => c.Story)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ChapterVM
                {
                    Id = c.Id,
                    StoryId = c.StoryId,
                    Name = c.Name,
                    ChapterNumber = c.ChapterNumber,
                    Slug = c.Slug,
                    ViewCount = c.ViewCount,
                    Content = c.Content,
                    ServerId = c.ServerId,
                    CreatedAt = c.CreatedAt,
                    Price = c.Price,
                    Discount = c.Discount
                })
                .ToListAsync();
            return View(chapters);
        }
    }
}