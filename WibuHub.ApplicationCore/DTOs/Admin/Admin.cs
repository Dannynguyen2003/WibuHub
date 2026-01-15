using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.ApplicationCore.DTOs.Admin
{
    public class Admin
    {
        public record CreateStoryDto(string Title, string Description, string CoverImageUrl, List<int> CategoryIds);

        // DTO sửa truyện
        public record UpdateStoryDto(string Title, string Description, string CoverImageUrl, bool IsCompleted);

        // DTO up chương (Giả sử nội dung là text hoặc link ảnh đã upload sẵn)
        public record UploadChapterDto(string Title, float ChapterNumber, string ContentUrl, decimal Price = 0);

        // DTO chỉnh giá
        public record SetPriceDto(decimal Price);

        // DTO rút tiền
        public record WithdrawDto(decimal Amount, string BankInfo);

        public class PublisherStatsDto
        {
            public int TotalStories { get; set; }
            public long TotalViews { get; set; }
            public int TotalFollowers { get; set; }
            public decimal MonthlyIncome { get; set; }
        }

        // DTO hiển thị danh sách truyện
        public class StoryDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string CoverImageUrl { get; set; }
            public StoryStatus Status { get; set; }
            public long ViewCount { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
