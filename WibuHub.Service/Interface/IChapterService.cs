using WibuHub.ApplicationCore.DTOs.Shared;

namespace WibuHub.Service.Interface
{
    public interface IChapterService
    {
        // Lấy tất cả (thường ít dùng, chủ yếu dùng GetByStoryId)
        Task<List<ChapterDto>> GetAllAsync();

        // Lấy chi tiết chapter (kèm danh sách ảnh)
        Task<ChapterDto?> GetByIdAsync(Guid id);

        // Lấy danh sách chapter của 1 bộ truyện
        Task<List<ChapterDto>> GetByStoryIdAsync(Guid storyId);

        // Tạo mới (Nhận DTO chứa File)
        Task<bool> CreateAsync(ChapterDto request);

        // Cập nhật
        Task<bool> UpdateAsync(Guid id, ChapterDto request);

        // Xóa
        Task<bool> DeleteAsync(Guid id);
    }
}