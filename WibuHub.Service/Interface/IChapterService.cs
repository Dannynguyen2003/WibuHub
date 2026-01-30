using WibuHub.ApplicationCore.Entities;
using WibuHub.MVC.ViewModels;

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
        Task<bool> CreateAsync(ChapterCreateVM request);

        // Cập nhật chapter
        Task<bool> UpdateAsync(Guid id, ChapterCreateVM request);

        // Xóa chapter (soft delete)
        Task<bool> DeleteAsync(Guid id);
    }
}
