using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
namespace WibuHub.Service.Interface
{
    public interface IChapterService
    {
        // Lấy danh sách tất cả chapters
        Task<List<Chapter>> GetAllAsync();
        // Lấy chi tiết chapter theo ID
        Task<Chapter?> GetByIdAsync(Guid id);
        // Lấy chapters theo StoryId
        Task<List<Chapter>> GetByStoryIdAsync(Guid storyId);
        // Tạo mới chapter
        Task<bool> CreateAsync(ChapterDto request);
        // Cập nhật chapter
        Task<bool> UpdateAsync(Guid id, ChapterDto request);
        // Xóa chapter (soft delete)
        Task<bool> DeleteAsync(Guid id);
    }
}