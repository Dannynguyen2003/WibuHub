using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations
{
    public class StoryService : IStoryService
    {
        private readonly StoryDbContext _context;
        private readonly ILogger<StoryService>? _logger;

        public StoryService(StoryDbContext context, ILogger<StoryService>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Story>> GetAllAsync()
        {
            return await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .OrderByDescending(s => s.UpdateDate)
                .ToListAsync();
        }

        public async Task<Story?> GetByIdAsync(Guid id)
        {
            return await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> CreateAsync(StoryVM request)
        {
            try
            {
                // Kiểm tra CategoryId có tồn tại không
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists)
                {
                    _logger?.LogWarning("Cannot create story: Category with ID {CategoryId} not found", request.CategoryId);
                    return false;
                }

                var story = new Story
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title.Trim(),
                    AlternativeName = request.AlternativeName?.Trim(),
                    Description = request.Description?.Trim(),
                    Thumbnail = request.Thumbnail?.Trim(),
                    Status = request.Status,
                    ViewCount = request.ViewCount,
                    FollowCount = request.FollowCount,
                    RatingScore = request.RatingScore,
                    DateCreated = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    AuthorId = request.AuthorId,
                    CategoryId = request.CategoryId
                };

                _context.Stories.Add(story);
                await _context.SaveChangesAsync();
                _logger?.LogInformation("Successfully created story with ID {StoryId}", story.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating story: {Title}", request.Title);
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, StoryVM request)
        {
            try
            {
                var story = await _context.Stories.FindAsync(id);
                if (story == null)
                {
                    _logger?.LogWarning("Cannot update story: Story with ID {StoryId} not found", id);
                    return false;
                }

                // Kiểm tra CategoryId có tồn tại không
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists)
                {
                    _logger?.LogWarning("Cannot update story: Category with ID {CategoryId} not found", request.CategoryId);
                    return false;
                }

                story.Title = request.Title.Trim();
                story.AlternativeName = request.AlternativeName?.Trim();
                story.Description = request.Description?.Trim();
                story.Thumbnail = request.Thumbnail?.Trim();
                story.Status = request.Status;
                story.ViewCount = request.ViewCount;
                story.FollowCount = request.FollowCount;
                story.RatingScore = request.RatingScore;
                story.AuthorId = request.AuthorId;
                story.CategoryId = request.CategoryId;
                story.UpdateDate = DateTime.UtcNow;

                _context.Update(story);
                await _context.SaveChangesAsync();
                _logger?.LogInformation("Successfully updated story with ID {StoryId}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger?.LogError(ex, "Concurrency error updating story with ID {StoryId}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating story with ID {StoryId}", id);
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
                    _logger?.LogWarning("Cannot delete story: Story with ID {StoryId} not found", id);
                    return false;
                }

                // Note: Remove() triggers soft delete through DbContext.SaveChangesAsync override
                // The DbContext intercepts Delete operations on ISoftDelete entities and sets IsDeleted=true instead
                _context.Stories.Remove(story);
                await _context.SaveChangesAsync();
                _logger?.LogInformation("Successfully soft-deleted story with ID {StoryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting story with ID {StoryId}", id);
                return false;
            }
        }
    }
}
