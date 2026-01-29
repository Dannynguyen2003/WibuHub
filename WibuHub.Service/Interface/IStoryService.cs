using WibuHub.ApplicationCore.Entities;
using WibuHub.MVC.ViewModels;

namespace WibuHub.Service.Interface
{
    public interface IStoryService
    {
        // Lấy danh sách tất cả truyện
        Task<List<Story>> GetAllAsync();

        // Lấy chi tiết truyện theo ID
        Task<Story?> GetByIdAsync(Guid id);

        // Tạo mới truyện
        Task<bool> CreateAsync(StoryCreateVM request);

        // Cập nhật truyện
        Task<bool> UpdateAsync(Guid id, StoryCreateVM request);

        // Xóa truyện
        Task<bool> DeleteAsync(Guid id);
    }
}
