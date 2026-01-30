using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.Service.Interface
{
    public interface IStoryService
    {
        // Lấy danh sách tất cả truyện
        Task<List<Story>> GetAllAsync();
        // Lấy chi tiết truyện theo ID
        Task<Story?> GetByIdAsync(Guid id);
        // Tạo mới truyện
        Task<bool> CreateAsync(StoryDto request);
        // Cập nhật truyện
        Task<bool> UpdateAsync(Guid id, StoryDto request);
        // Xóa truyện
        Task<bool> DeleteAsync(Guid id);
    }
}