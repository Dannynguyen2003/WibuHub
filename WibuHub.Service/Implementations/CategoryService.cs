using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.MVC.ViewModels;
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

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.OrderBy(x => x.Position).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<CategoryVM?> GetByIdAsViewModelAsync(Guid id)
        {
            return await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Position = c.Position
                })
                .SingleOrDefaultAsync();
        }

        public async Task<bool> CreateAsync(CategoryVM categoryVM)
        {
            // Logic kiểm tra trùng tên
            var exists = await _context.Categories.AnyAsync(c => c.Name == categoryVM.Name);
            if (exists) return false;

            // Logic tính toán Position
            var countCategory = await _context.Categories.CountAsync();

            var category = new Category
            {
                Id = Guid.NewGuid(), // Đảm bảo ID được tạo
                Name = categoryVM.Name.Trim(),
                Description = categoryVM.Description?.Trim(),
                Position = countCategory + 1
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(CategoryVM categoryVM)
        {
            var category = await _context.Categories
                .SingleOrDefaultAsync(c => c.Id == categoryVM.Id);

            if (category == null) return false;

            category.Name = categoryVM.Name.Trim();
            category.Description = categoryVM.Description?.Trim();
            // Lưu ý: Không update Position ở đây theo logic cũ của bạn

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<(bool isSuccess, string message)> DeleteAsync(Guid id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return (false, "Không tìm thấy danh mục!");
                }

                // Logic sắp xếp lại vị trí (Reordering)
                var listCategory = await _context.Categories
                    .Where(c => c.Position > category.Position)
                    .ToListAsync();

                foreach (var cat in listCategory)
                {
                    cat.Position--;
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return (true, "Xóa thành công!");
            }
            catch (Exception ex)
            {
                // Có thể log error tại đây: _logger.LogError(ex, ...);
                return (false, "Lỗi thực thi: " + ex.Message);
            }
        }
    }
}