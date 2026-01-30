using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
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

        public async Task<List<StoryDto>> GetAllAsync()
        {
            return await _context.Stories
                .Include(s => s.Category) // Join bảng Category để lấy tên
                .Select(s => new StoryDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    AuthorName = s.AuthorName,
                    CoverImage = s.CoverImage,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category != null ? s.Category.Name : "N/A"
                })
                .OrderByDescending(s => s.Title) // Sắp xếp tùy ý
                .ToListAsync();
        }

        public async Task<StoryDto?> GetByIdAsync(Guid id)
        {
            var story = await _context.Stories
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (story == null) return null;

            return new StoryDto
            {
                Id = story.Id,
                Title = story.Title,
                Description = story.Description,
                AuthorName = story.AuthorName,
                CoverImage = story.CoverImage,
                CategoryId = story.CategoryId,
                CategoryName = story.Category?.Name
            };
        }

        public async Task<bool> CreateAsync(StoryDto dto)
        {
            try
            {
                var entity = new Story
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    AuthorName = dto.AuthorName,
                    CoverImage = dto.CoverImage,
                    Status = (int)StoryStatus.Ongoing,
                    CategoryId = dto.CategoryId,
                    CreatedAt = DateTime.Now
                };

                _context.Stories.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, StoryDto dto)
        {
            var entity = await _context.Stories.FindAsync(id);
            if (entity == null) return false;

            // Cập nhật dữ liệu
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.AuthorName = dto.AuthorName;
            entity.CategoryId = dto.CategoryId;

            // Nếu có upload ảnh mới thì mới cập nhật ảnh, không thì giữ nguyên
            if (!string.IsNullOrEmpty(dto.CoverImage))
            {
                entity.CoverImage = dto.CoverImage;
            }

            _context.Stories.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Stories.FindAsync(id);
            if (entity == null) return false;

            _context.Stories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}