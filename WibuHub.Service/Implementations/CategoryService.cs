using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly StoryDbContext _context;

        public CategoryService(StoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<bool> CreateAsync(CategoryDto request)
        {
            // Kiểm tra trùng tên
            bool isExists = await _context.Categories.AnyAsync(c => c.Name == request.Name);
            if (isExists) return false;

            try
            {
                var entity = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description
                };

                _context.Categories.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, CategoryDto request)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null) return false;

            // Kiểm tra trùng tên nhưng bỏ qua chính nó
            bool isExists = await _context.Categories
                .AnyAsync(c => c.Name == request.Name && c.Id != id);

            if (isExists) return false;

            entity.Name = request.Name;
            entity.Description = request.Description;

            _context.Categories.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool isSuccess, string message)> DeleteAsync(Guid id)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null)
                return (false, "Không tìm thấy danh mục");

            // Kiểm tra xem có truyện nào đang dùng danh mục này không?
            // (Nếu xóa danh mục đang có truyện sẽ gây lỗi khóa ngoại hoặc mất dữ liệu truyện)
            bool hasStories = await _context.Stories.AnyAsync(s => s.CategoryId == id);
            if (hasStories)
            {
                return (false, "Không thể xóa danh mục này vì đang có truyện sử dụng.");
            }

            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
            return (true, "Xóa thành công");
        }
    }
}