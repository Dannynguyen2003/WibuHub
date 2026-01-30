using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.Service.Interface
{
    public interface IChapterService
    {
        Task<List<Chapter>> GetAllAsync();
        Task<List<Chapter>> GetByStoryIdAsync(Guid storyId);
        Task<Chapter?> GetByIdAsync(Guid id);
        Task<ChapterDto?> GetByIdAsDtoAsync(Guid id);
        Task<bool> CreateAsync(ChapterDto chapterDto);
        Task<bool> UpdateAsync(ChapterDto chapterDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
