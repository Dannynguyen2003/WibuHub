using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;
namespace WibuHub.Service.Implementations
{
    public class ChapterService : IChapterService
    {
        private readonly StoryDbContext _context;
        public ChapterService(StoryDbContext context)
        {
            _context = context;
        }
        public async Task<List<Chapter>> GetAllAsync()
        {
            return await _context.Chapters
                .Include(c => c.Story)
                .OrderByDescending(c => c.CreateDate)
                .ToListAsync();
        }
        public async Task<Chapter?> GetByIdAsync(Guid id)
        {
            return await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<List<Chapter>> GetByStoryIdAsync(Guid storyId)
        {
            return await _context.Chapters
                .Include(c => c.Story)
                .Where(c => c.StoryId == storyId)
                .OrderBy(c => c.Number)
                .ToListAsync();
        }
        public async Task<bool> CreateAsync(ChapterDto request)
        {
            try
            {
                // Validate StoryId exists
                var storyExists = await _context.Stories.AnyAsync(s => s.Id == request.StoryId);
                if (!storyExists)
                {
                    return false;
                }
                // Check if slug is unique
                var slugExists = await _context.Chapters.AnyAsync(c => c.Slug == request.Slug);
                if (slugExists)
                {
                    return false;
                }
                var chapter = new Chapter
                {
                    Id = Guid.NewGuid(),
                    StoryId = request.StoryId,
                    Name = (request.Name ?? string.Empty).Trim(),
                    Number = request.Number,
                    Slug = (request.Slug ?? string.Empty).Trim(),
                    Content = request.Content?.Trim(),
                    ServerId = request.ServerId,
                    Price = request.Price,
                    Discount = request.Discount,
                    CreateDate = DateTime.UtcNow,
                    ViewCount = 0
                };
                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> UpdateAsync(Guid id, ChapterDto request)
        {
            try
            {
                var chapter = await _context.Chapters.FindAsync(id);
                if (chapter == null)
                {
                    return false;
                }
                // Validate StoryId exists
                var storyExists = await _context.Stories.AnyAsync(s => s.Id == request.StoryId);
                if (!storyExists)
                {
                    return false;
                }
                // Check if slug is unique (excluding current chapter)
                var slugExists = await _context.Chapters
                    .AnyAsync(c => c.Slug == request.Slug && c.Id != id);
                if (slugExists)
                {
                    return false;
                }
                // Update properties
                chapter.StoryId = request.StoryId;
                chapter.Name = (request.Name ?? string.Empty).Trim();
                chapter.Number = request.Number;
                chapter.Slug = (request.Slug ?? string.Empty).Trim();
                chapter.Content = request.Content?.Trim();
                chapter.ServerId = request.ServerId;
                chapter.Price = request.Price;
                chapter.Discount = request.Discount;
                // EF Core change tracker will automatically detect changes
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
                var chapter = await _context.Chapters.FindAsync(id);
                if (chapter == null)
                {
                    return false;
                }
                // This will trigger soft delete via DbContext.SaveChangesAsync override
                _context.Chapters.Remove(chapter);
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
