using WibuHub.ApplicationCore.DTOs.Shared;

namespace WibuHub.Service.Interface
{
    public interface ICategoryService
    {
        // Trả về List DTO
        Task<List<CategoryDto>> GetAllAsync();

        // Lấy chi tiết trả về DTO
        Task<CategoryDto?> GetByIdAsync(Guid id);

        Task<CategoryDto?> GetByNameAsync(string name);

        // Tạo mới nhận DTO
        Task<bool> CreateAsync(CategoryDto request);

        // Cập nhật nhận ID và DTO
        Task<bool> UpdateAsync(Guid id, CategoryDto request);

        // Xóa
        Task<(bool isSuccess, string message)> DeleteAsync(Guid id);
    }
}
