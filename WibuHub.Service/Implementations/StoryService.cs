using Humanizer;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations
{
    public class StoryService : IStoryService
    {
        private readonly StoryDbContext _context;

        public StoryService(StoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<StoryDto>> GetAllAsync()
        {
            var stories = await _context.Stories
                .Select(s => new StoryDto
                {
                    Id = s.Id,
                    Title = s.StoryName,
                    Description = s.Description,
                    AuthorName = s.AuthorName,
                    CoverImage = s.CoverImage,
                    TotalChapters = s.Chapters.Count(),
                    LatestChapter = s.LatestChapter,
                    Price = s.Price,
                    Discount = s.Discount,
                    Categories = s.StoryCategories.Select(sc => new CategoryInfoDto
                    {
                        Id = sc.CategoryId,
                        Name = sc.Category.Name ?? "N/A"
                    }).ToList(),
                    ViewCount = s.ViewCount,
                    CreatedAt = s.CreatedAt
                })
                .OrderByDescending(s => s.Title)
                .ToListAsync();

            foreach (var story in stories)
            {
                story.TimeAgo = GetTimeAgo(story.CreatedAt);
            }
            return stories;
        }

        public async Task<StoryDto?> GetByIdAsync(Guid id)
        {
            var story = await _context.Stories
                .Include(s => s.StoryCategories).ThenInclude(sc => sc.Category)
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (story == null) return null;

            return new StoryDto
            {
                Id = story.Id,
                Title = story.StoryName,
                Description = story.Description,
                AuthorName = story.AuthorName,
                CoverImage = story.CoverImage,
                Price = story.Price,
                Discount = story.Discount,
                Categories = story.StoryCategories.Select(sc => new CategoryInfoDto
                {
                    Id = sc.CategoryId,
                    Name = sc.Category?.Name ?? "N/A"
                }).ToList(),
                ViewCount = story.ViewCount,
                CategoryIds = story.StoryCategories.Select(sc => sc.CategoryId).ToList(),
                TotalChapters = story.Chapters.Count,
                LatestChapter = story.LatestChapter,
                CreatedAt = story.CreatedAt,
                TimeAgo = GetTimeAgo(story.CreatedAt)
            };
        }

        public async Task<List<StoryDto>> GetNewestStoriesAsync()
        {
            var newestStories = await _context.Stories
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new StoryDto
                {
                    Id = s.Id,
                    Title = s.StoryName,
                    CoverImage = s.CoverImage,
                    Categories = s.StoryCategories.Select(sc => new CategoryInfoDto
                    {
                        Id = sc.CategoryId,
                        Name = sc.Category.Name ?? "N/A"
                    }).ToList(),
                    ViewCount = s.ViewCount,
                    CreatedAt = s.CreatedAt,
                    TotalChapters = s.Chapters.Count()
                })
                .ToListAsync();

            foreach (var story in newestStories)
            {
                story.TimeAgo = GetTimeAgo(story.CreatedAt);
            }
            return newestStories;
        }

        public async Task<IEnumerable<StoryDto>> GetTopViewsAsync(int take = 5)
        {
            var topStories = await _context.Stories
                .OrderByDescending(s => s.ViewCount)
                .Take(take)
                .Select(s => new StoryDto
                {
                    Id = s.Id,
                    Title = s.StoryName,
                    ViewCount = s.ViewCount,
                    CoverImage = s.CoverImage,
                    AuthorName = s.AuthorName ?? "Đang cập nhật",
                    Categories = s.StoryCategories.Select(sc => new CategoryInfoDto
                    {
                        Id = sc.CategoryId,
                        Name = sc.Category.Name ?? "N/A"
                    }).ToList(),
                    Description = s.Description,
                    TotalChapters = s.Chapters.Count(),
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            foreach (var story in topStories)
            {
                story.TimeAgo = GetTimeAgo(story.CreatedAt);
            }
            return topStories;
        }
        public async Task<IEnumerable<object>> SearchSuggestAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<object>();

            keyword = keyword.ToLower();

            var suggestions = await _context.Stories
                .Where(x => x.StoryName.ToLower().Contains(keyword)) // Đã fix lỗi dư dấu chấm ở đây
                .Select(x => new
                {
                    id = x.Id,
                    title = x.StoryName,
                    coverImage = x.CoverImage,
                    author = x.AuthorName != null ? x.AuthorName : "Đang cập nhật",
                    chapter = x.TotalChapters
                })
                .Take(5)
                .ToListAsync();

            return suggestions;
        }
        public async Task<bool> CreateAsync(StoryDto dto)
        {
            try
            {
                var entity = new Story
                {
                    Id = Guid.NewGuid(),
                    StoryName = dto.Title,
                    Description = dto.Description,
                    AuthorName = dto.AuthorName,
                    CoverImage = dto.CoverImage,
                    Price = dto.Price,
                    Discount = dto.Discount,
                    Status = (int)StoryStatus.Ongoing,
                    StoryCategories = dto.CategoryIds.Select(catId => new StoryCategory
                    {
                        CategoryId = catId
                    }).ToList(),
                    CreatedAt = DateTime.Now
                };

                _context.Stories.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, StoryDto dto)
        {
            // ĐÃ SỬA: Include danh sách thể loại để Clear() không bị lỗi
            var entity = await _context.Stories
                .Include(s => s.StoryCategories)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null) return false;

            entity.StoryName = dto.Title;
            entity.Description = dto.Description;
            entity.AuthorName = dto.AuthorName;

            // Xóa danh sách cũ và cập nhật danh sách mới
            entity.StoryCategories.Clear();
            foreach (var catId in dto.CategoryIds)
            {
                entity.StoryCategories.Add(new StoryCategory { CategoryId = catId });
            }

            entity.Price = dto.Price;
            entity.Discount = dto.Discount;

            if (!string.IsNullOrEmpty(dto.CoverImage))
            {
                entity.CoverImage = dto.CoverImage;
            }

            _context.Stories.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Stories.FindAsync(id);
            if (entity == null) return false;

            _context.Stories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            if (timeSpan <= TimeSpan.FromSeconds(60)) return "Vừa xong";
            if (timeSpan <= TimeSpan.FromMinutes(60)) return timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} phút trước" : "1 phút trước";
            if (timeSpan <= TimeSpan.FromHours(24)) return timeSpan.Hours > 1 ? $"{timeSpan.Hours} giờ trước" : "1 giờ trước";
            if (timeSpan <= TimeSpan.FromDays(30)) return timeSpan.Days > 1 ? $"{timeSpan.Days} ngày trước" : "1 ngày trước";
            if (timeSpan <= TimeSpan.FromDays(365)) return timeSpan.Days > 30 ? $"{timeSpan.Days / 30} tháng trước" : "1 tháng trước";
            return timeSpan.Days > 365 ? $"{timeSpan.Days / 365} năm trước" : "1 năm trước";
        }

        public async Task<List<StoryDto>> GetStoriesByGenreAsync(Guid genreId)
        {
            return await _context.Stories
                .Where(s => s.StoryCategories.Any(sc => sc.CategoryId == genreId))
                .Select(s => new StoryDto
                {
                    Id = s.Id, // ĐÃ SỬA: Lấy đúng ID thật của truyện thay vì Guid.NewGuid()
                    Title = s.StoryName,
                    Description = s.Description,
                    AuthorName = s.AuthorName,
                    CoverImage = s.CoverImage,
                    Price = s.Price,
                    Discount = s.Discount,
                    Status = (int)StoryStatus.Ongoing,
                    CreatedAt = s.CreatedAt,
                    Categories = s.StoryCategories.Select(sc => new CategoryInfoDto
                    {
                        Id = sc.CategoryId,
                        Name = sc.Category.Name ?? "N/A"
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}