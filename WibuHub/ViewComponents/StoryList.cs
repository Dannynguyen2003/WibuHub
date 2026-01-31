using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var stories = await _context.Stories
                .Where(s => !s.IsDeleted)
                .Include(s => s.Author)
                .Include(s => s.Category)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new StoryVM
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
                    CategoryName = s.Category != null ? s.Category.Name : null
                })
                .ToListAsync();
            return View(stories);
        }
    }
}