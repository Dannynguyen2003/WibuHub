using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
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
        public async Task<IViewComponentResult> InvokeAsync(int page = 1, int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Images);

            var totalCount = await query.LongCountAsync();

            var chapters = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ChapterVM
                {
                    Id = c.Id,
                    StoryId = c.StoryId,
                    StoryName = c.Story != null ? c.Story.StoryName : string.Empty,
                    Name = c.Name,
                    ChapterNumber = c.ChapterNumber,
                    Slug = c.Slug,
                    ViewCount = c.ViewCount,
                    Content = c.Content,
                    ImageCount = c.Images.Count,
                    ServerId = c.ServerId,
                    CreatedAt = c.CreatedAt,
                    Price = c.Price,
                    Discount = c.Discount
                })
                .ToListAsync();

            var result = new PagedResult<ChapterVM>(chapters, page, pageSize, totalCount);
            return View(result);
        }
    }
}