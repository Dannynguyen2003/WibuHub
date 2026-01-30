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

        public async Task<List<Chapter>> GetByStoryIdAsync(Guid storyId)
        {
            return await _context.Chapters
                .Where(c => c.StoryId == storyId)
                .OrderBy(c => c.Number)
                .ToListAsync();
        }

        public async Task<Chapter?> GetByIdAsync(Guid id)
        {
            return await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<ChapterDto?> GetByIdAsDtoAsync(Guid id)
        {
            return await _context.Chapters
                .Where(c => c.Id == id)
                .Select(c => new ChapterDto
                {
                    Id = c.Id,
                    StoryId = c.StoryId,
                    StoryTitle = c.Story != null ? c.Story.Title : null,
                    Name = c.Name,
                    Number = c.Number,
                    Slug = c.Slug,
                    ViewCount = c.ViewCount,
                    Content = c.Content,
                    ServerId = c.ServerId,
                    CreateDate = c.CreateDate,
                    Price = c.Price,
                    Discount = c.Discount
                })
                .SingleOrDefaultAsync();
        }

        public async Task<bool> CreateAsync(ChapterDto chapterDto)
        {
            try
            {
                // Kiểm tra StoryId có tồn tại không
                var storyExists = await _context.Stories.AnyAsync(s => s.Id == chapterDto.StoryId);
                if (!storyExists) return false;

                var chapter = new Chapter
                {
                    Id = Guid.NewGuid(),
                    StoryId = chapterDto.StoryId,
                    Name = chapterDto.Name.Trim(),
                    Number = chapterDto.Number,
                    Slug = chapterDto.Slug.Trim(),
                    ViewCount = chapterDto.ViewCount,
                    Content = chapterDto.Content?.Trim(),
                    ServerId = chapterDto.ServerId,
                    CreateDate = DateTime.UtcNow,
                    Price = chapterDto.Price,
                    Discount = chapterDto.Discount
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

        public async Task<bool> UpdateAsync(ChapterDto chapterDto)
        {
            try
            {
                var chapter = await _context.Chapters.FindAsync(chapterDto.Id);
                if (chapter == null) return false;

                // Kiểm tra StoryId có tồn tại không
                var storyExists = await _context.Stories.AnyAsync(s => s.Id == chapterDto.StoryId);
                if (!storyExists) return false;

                chapter.StoryId = chapterDto.StoryId;
                chapter.Name = chapterDto.Name.Trim();
                chapter.Number = chapterDto.Number;
                chapter.Slug = chapterDto.Slug.Trim();
                chapter.ViewCount = chapterDto.ViewCount;
                chapter.Content = chapterDto.Content?.Trim();
                chapter.ServerId = chapterDto.ServerId;
                chapter.Price = chapterDto.Price;
                chapter.Discount = chapterDto.Discount;

                _context.Update(chapter);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var chapter = await _context.Chapters.FindAsync(id);
                if (chapter == null) return false;

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
