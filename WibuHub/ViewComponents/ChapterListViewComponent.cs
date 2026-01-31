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
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Chapters
                .Include(c => c.Story)
                .OrderByDescending(c => c.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paged data
            var chapters = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            var pagedResult = new PagedResult<ChapterVM>(chapters, page, pageSize, totalCount);

            return View(pagedResult);
        }
    }
}
