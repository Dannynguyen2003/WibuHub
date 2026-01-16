//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static WibuHub.ApplicationCore.DTOs.Admin.Admin;

//namespace WibuHub.ApplicationCore.Interface
//{
//    public interface IContentManagementService
//    {
//        // 1. Thống kê
//        Task<StatsDto> GetCreatorStatsAsync(string userId);

//        // 2. Quản lý Truyện
//        Task<PagedResult<StoryDto>> GetStoriesByCreatorAsync(string userId, int page, int pageSize = 10);
//        Task<int> CreateStoryAsync(string userId, CreateStoryDto request);
//        Task UpdateStoryAsync(int storyId, UpdateStoryDto request);
//        Task<bool> IsStoryOwnerAsync(int storyId, string userId); // Hàm check quyền quan trọng

//        // 3. Quản lý Chapter
//        Task AddChapterAsync(int storyId, UploadChapterDto request);
//        Task<ServiceResult> SetChapterPriceAsync(string userId, int chapterId, decimal price);
//    }
//}
