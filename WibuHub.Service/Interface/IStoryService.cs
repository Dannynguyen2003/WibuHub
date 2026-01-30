using WibuHub.ApplicationCore.DTOs.Shared; // Nhớ using namespace chứa DTO

namespace WibuHub.Service.Interface
{
    public interface IStoryService
    {
        // Trả về List DTO để hiển thị, không trả Entity
        Task<List<StoryDto>> GetAllAsync();

        // Lấy chi tiết cũng trả về DTO
        Task<StoryDto?> GetByIdAsync(Guid id);

        // Create nhận vào DTO (hoặc StoryCreateDto nếu bạn tách riêng)
        Task<bool> CreateAsync(StoryDto request);

        // Update nhận vào DTO
        Task<bool> UpdateAsync(Guid id, StoryDto request);

        Task<bool> DeleteAsync(Guid id);
    }
}