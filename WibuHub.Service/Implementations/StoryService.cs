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
                    Title = s.StoryName,
                    Description = s.Description,
                    AuthorName = s.AuthorName,
                    CoverImage = s.CoverImage,
                    Price = s.Price,
                    Discount = s.Discount,
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
                Title = story.StoryName,
                Description = story.Description,
                AuthorName = story.AuthorName,
                CoverImage = story.CoverImage,
                Price = story.Price,
                Discount = story.Discount,
                CategoryId = story.CategoryId,
                CategoryName = story.Category?.Name
            };
        }
        public async Task<List<StoryDto>> GetNewestStoriesAsync()
        {
            // Giả sử _context là ApplicationDbContext của bạn
            var newestStories = await _context.Stories
                .OrderByDescending(s => s.CreatedAt) // Sắp xếp giảm dần theo ngày tạo
                .Take(5) // CHỈ LẤY TỐI ĐA 5 TRUYỆN TỪ DATABASE
                .Select(s => new StoryDto
                {
                    Id = s.Id,
                    Title = s.StoryName,
                    CoverImage = s.CoverImage,
                    CategoryName = s.Category.Name,
                    ViewCount = s.ViewCount,
                    //Rating = s.Rating,
                    // Format ngày tạo để hiển thị "2 giờ trước", "01-12"...
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return newestStories;
        }

        public async Task<IEnumerable<StoryDto>> GetTopViewsAsync(int take = 5)
        {
            // Tùy vào việc bạn dùng _context hay _repository, hãy thay thế cho phù hợp nhé
            var topStories = await _context.Stories // Hoặc _storyRepository.GetAll()
                .OrderByDescending(s => s.ViewCount)    // Sắp xếp lượt xem từ cao xuống thấp
                .Take(take)                         // Lấy số lượng tương ứng (5 truyện)
                .Select(s => new StoryDto           // Map sang DTO để trả về
                {
                    Id = s.Id,
                    Title = s.StoryName,
                    ViewCount = s.ViewCount,
                    // Map thêm các trường cần thiết để giao diện hiển thị Popup chi tiết
                    CoverImage = s.CoverImage,
                    AuthorName = s.AuthorName ?? "Đang cập nhật",
                    CategoryName = s.Category != null ? s.Category.Name : "Đang cập nhật",
                    //Rating = s.Rating ?? 5.0,
                    Description = s.Description
                    // (Bạn map các trường giống hệt hàm GetNewestStoriesAsync là chuẩn nhất)
                })
                .ToListAsync();

            return topStories;
        }

        public async Task<bool> CreateAsync(StoryDto dto)
        {
            try
            {
                var entity = new Story
                {
                    Id = Guid.NewGuid(),
                    StoryName = dto.Title,
                    Description = dto.Description,
                    AuthorName = dto.AuthorName,
                    CoverImage = dto.CoverImage,
                    Price = dto.Price,
                    Discount = dto.Discount,
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
            entity.StoryName = dto.Title;
            entity.Description = dto.Description;
            entity.AuthorName = dto.AuthorName;
            entity.CategoryId = dto.CategoryId;
            entity.Price = dto.Price;
            entity.Discount = dto.Discount;

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