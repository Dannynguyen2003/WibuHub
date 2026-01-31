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
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Stories
                .Where(s => !s.IsDeleted)
                .Include(s => s.Author)
                .Include(s => s.Category)
                .OrderByDescending(s => s.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paged data
            var stories = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to ViewModel
            var storyVMs = stories.Select(s => new StoryVM
            {
                Id = s.Id,
                Title = s.StoryName,
                AlternativeName = s.AlternativeName,
                Description = s.Description,
                Slug = s.Slug,
                Status = s.Status,
                ViewCount = s.ViewCount,
                FollowCount = s.FollowCount,
                RatingScore = s.RatingScore,
                CreatedAt = s.CreatedAt,
                UpdateDate = s.UpdateDate,
                AuthorId = s.AuthorId,
                CategoryId = s.CategoryId,
                Author = s.Author,
                CategoryName = s.Category != null ? s.Category.Name : null
            }).ToList();

            var pagedResult = new PagedResult<StoryVM>(storyVMs, page, pageSize, totalCount);

            return View(pagedResult);
        }
    }
}
