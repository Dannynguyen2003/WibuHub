using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations
{
    public class StoryService : IStoryService
    {
        private readonly StoryDbContext _context;

        public StoryService(StoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Story>> GetAllAsync()
        {
            return await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();
        }

        public async Task<Story?> GetByIdAsync(Guid id)
        {
            return await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<StoryVM?> GetByIdAsViewModelAsync(Guid id)
        {
            return await _context.Stories
                .Where(s => s.Id == id)
                .Select(s => new StoryVM
                {
                    Id = s.Id,
                    Title = s.Title,
                    AlternativeName = s.AlternativeName,
                    Description = s.Description,
                    Thumbnail = s.Thumbnail,
                    Status = s.Status,
                    ViewCount = s.ViewCount,
                    FollowCount = s.FollowCount,
                    RatingScore = s.RatingScore,
                    DateCreated = s.DateCreated,
                    UpdateDate = s.UpdateDate,
                    AuthorId = s.AuthorId,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category != null ? s.Category.Name : null
                })
                .SingleOrDefaultAsync();
        }

        public async Task<bool> CreateAsync(StoryCreateVM request)
        {
            try
            {
                // Validate CategoryId exists
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists)
                {
                    return false;
                }

                // Validate AuthorId if provided
                if (request.AuthorId.HasValue)
                {
                    var authorExists = await _context.Authors.AnyAsync(a => a.Id == request.AuthorId.Value);
                    if (!authorExists)
                    {
                        return false;
                    }
                }

                var story = new Story
                {
                    Id = Guid.NewGuid(),
                    Title = (request.Title ?? string.Empty).Trim(),
                    AlternativeName = request.AlternativeName?.Trim(),
                    Description = request.Description?.Trim(),
                    Thumbnail = request.Thumbnail?.Trim(),
                    Status = request.Status,
                    CategoryId = request.CategoryId,
                    AuthorId = request.AuthorId,
                    DateCreated = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    ViewCount = 0,
                    FollowCount = 0,
                    RatingScore = 0
                };

                _context.Stories.Add(story);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, StoryCreateVM request)
        {
            try
            {
                var story = await _context.Stories.FindAsync(id);
                if (story == null)
                {
                    return false;
                }

                // Validate CategoryId exists
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists)
                {
                    return false;
                }

                // Validate AuthorId if provided
                if (request.AuthorId.HasValue)
                {
                    var authorExists = await _context.Authors.AnyAsync(a => a.Id == request.AuthorId.Value);
                    if (!authorExists)
                    {
                        return false;
                    }
                }

                // Update properties
                story.Title = (request.Title ?? string.Empty).Trim();
                story.AlternativeName = request.AlternativeName?.Trim();
                story.Description = request.Description?.Trim();
                story.Thumbnail = request.Thumbnail?.Trim();
                story.Status = request.Status;
                story.CategoryId = request.CategoryId;
                story.AuthorId = request.AuthorId;
                story.UpdateDate = DateTime.UtcNow;

                // EF Core change tracker will automatically detect changes, no need for explicit Update()
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var story = await _context.Stories.FindAsync(id);
                if (story == null)
                {
                    return false;
                }

                // This will trigger soft delete via DbContext.SaveChangesAsync override
                _context.Stories.Remove(story);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
