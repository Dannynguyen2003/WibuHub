using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.Service.Interface
{
    public interface ICategoryService
    {
        // Lấy danh sách hiển thị
        Task<List<Category>> GetAllAsync();

        // Lấy chi tiết (Entity)
        Task<Category?> GetByIdAsync(Guid id);

        // Lấy dữ liệu để đổ vào Form Edit (DTO)
        Task<CategoryDto?> GetByIdAsDtoAsync(Guid id);

        // Tạo mới (trả về true nếu thành công, false nếu trùng tên hoặc lỗi)
        Task<bool> CreateAsync(CategoryDto categoryDto);

        // Cập nhật
        Task<bool> UpdateAsync(CategoryDto categoryDto);

        // Xóa (trả về trạng thái và message để Controller trả về Json)
        Task<(bool isSuccess, string message)> DeleteAsync(Guid id);
    }
}