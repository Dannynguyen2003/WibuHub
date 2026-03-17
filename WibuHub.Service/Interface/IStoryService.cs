using WibuHub.ApplicationCore.DTOs.Shared;

namespace WibuHub.Service.Interface
{
    public interface IStoryService
    {
        Task<List<StoryDto>> GetAllAsync();

        Task<StoryDto?> GetByIdAsync(Guid id);

        Task<bool> CreateAsync(StoryDto request);

        Task<bool> UpdateAsync(Guid id, StoryDto request);

        Task<bool> DeleteAsync(Guid id);

        // SỬA LẠI DÒNG NÀY: Trả về danh sách (List) các StoryDto
        Task<List<StoryDto>> GetNewestStoriesAsync();
    }
}