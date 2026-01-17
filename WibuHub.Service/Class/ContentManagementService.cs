using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities; // Nơi chứa Story, Chapter
using WibuHub.ApplicationCore.Interface;
using WibuHub.DataLayer;
using static WibuHub.ApplicationCore.DTOs.Admin.Admin; // Nơi chứa StoryDbContext

namespace WibuHub.Service
{
    public class ContentManagementService : IContentManagementService
    {
        private readonly StoryDbContext _context;

        public ContentManagementService(StoryDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // 1. THỐNG KÊ (STATS)
        // ==========================================================
        public async Task<StatsDto> GetCreatorStatsAsync(string userId)
        {
            // Lấy tất cả truyện của user này
            var stories = _context.Stories.Where(s => s.OwnerId == userId);

            // Tính toán số liệu
            var totalStories = await stories.CountAsync();

            // Tổng view của tất cả các truyện cộng lại
            var totalViews = await stories.SumAsync(s => s.ViewCount);

            // Tổng người theo dõi
            var totalFollowers = await _context.UserFollows
                .Where(f => f.Story.OwnerId == userId)
                .CountAsync();

            // Doanh thu tháng này (Giả sử có bảng RevenueReports)
            // Nếu chưa làm bảng Doanh thu thì tạm thời để 0 hoặc query bảng Transactions
            var monthlyIncome = 0;

            return new StatsDto
            {
                TotalStories = totalStories,
                TotalViews = totalViews,
                TotalFollowers = totalFollowers,
                MonthlyIncome = monthlyIncome
            };
        }

        // ==========================================================
        // 2. QUẢN LÝ TRUYỆN
        // ==========================================================
        public async Task<PagedResult<StoryDto>> GetStoriesByCreatorAsync(string userId, int page, int pageSize = 10)
        {
            var query = _context.Stories
                .Where(s => s.OwnerId == userId)
                .OrderByDescending(s => s.UpdatedAt); // Truyện mới cập nhật lên đầu

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StoryDto // Map Entity sang DTO
                {
                    Id = s.Id,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImageUrl,
                    Status = s.Status, // Ongoing, Completed...
                    ViewCount = s.ViewCount,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return new PagedResult<StoryDto>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<int> CreateStoryAsync(string userId, CreateStoryDto request)
        {
            var newStory = new Story
            {
                Title = request.Title,
                Description = request.Description,
                CoverImageUrl = request.CoverImageUrl,
                OwnerId = userId, // QUAN TRỌNG: Gán chủ sở hữu
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = StoryStatus.Ongoing,
                ViewCount = 0
            };

            // Xử lý Category (Thể loại)
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                foreach (var catId in request.CategoryIds)
                {
                    // Giả sử có bảng trung gian StoryCategories
                    newStory.StoryCategories.Add(new StoryCategory { CategoryId = catId });
                }
            }

            _context.Stories.Add(newStory);
            await _context.SaveChangesAsync();

            return newStory.Id;
        }

        public async Task UpdateStoryAsync(int storyId, UpdateStoryDto request)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null) return; // Hoặc throw exception

            story.Title = request.Title;
            story.Description = request.Description;
            story.CoverImageUrl = request.CoverImageUrl;
            story.Status = request.IsCompleted ? StoryStatus.Completed : StoryStatus.Ongoing;
            story.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsStoryOwnerAsync(int storyId, string userId)
        {
            // Kiểm tra xem truyện ID này có phải của User ID này không
            return await _context.Stories
                .AnyAsync(s => s.Id == storyId && s.OwnerId == userId);
        }

        // ==========================================================
        // 3. QUẢN LÝ CHAPTER
        // ==========================================================
        public async Task AddChapterAsync(int storyId, UploadChapterDto request)
        {
            var newChapter = new Chapter
            {
                StoryId = storyId,
                Title = request.Title,
                ChapterNumber = request.ChapterNumber,
                ContentUrl = request.ContentUrl, // Link ảnh hoặc nội dung text
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            _context.Chapters.Add(newChapter);

            // Cập nhật ngày mới nhất cho truyện để nó nổi lên trang chủ
            var story = await _context.Stories.FindAsync(storyId);
            if (story != null)
            {
                story.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ServiceResult> SetChapterPriceAsync(string userId, int chapterId, decimal price)
        {
            // Cần join bảng để check xem Chapter này thuộc Story nào, và Story đó có phải của User này không
            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .FirstOrDefaultAsync(c => c.Id == chapterId);

            if (chapter == null) return ServiceResult.Fail("Chapter không tồn tại");

            // Check quyền sở hữu sâu
            if (chapter.Story.OwnerId != userId)
                return ServiceResult.Fail("Bạn không có quyền sửa chapter này");

            chapter.Price = price;
            await _context.SaveChangesAsync();

            return ServiceResult.Ok();
        }
    }
}