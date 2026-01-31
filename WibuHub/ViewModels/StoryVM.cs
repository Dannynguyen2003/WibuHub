using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.MVC.ViewModels
{
    [Bind("Id,Title,AlternativeName,Description,Slug,Status,ViewCount,FollowCount,RatingScore,CreatedAt,UpdateDate,AuthorId,CategoryId")]
    public class StoryVM
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Title: nvarchar(255)
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        // AlternativeName: nvarchar(500)
        [MaxLength(500)]
        public string? AlternativeName { get; set; }

        // Description: nvarchar(MAX)
        // Trong EF Core, string không giới hạn MaxLength mặc định sẽ là nvarchar(MAX)
        public string? Description { get; set; }

        // Slug: varchar(150) - URL thân thiện
        [Required]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string Slug { get; set; } = string.Empty;

        // Status: tinyint (0: Đang tiến hành, 1: Hoàn thành, 2: Tạm ngưng)
        // Map int của C# sang tinyint của SQL
        [Column(TypeName = "tinyint")]
        public int Status { get; set; } = 0;

        // ViewCount: bigint
        public long ViewCount { get; set; } = 0;

        // FollowCount: int
        public int FollowCount { get; set; } = 0;

        // RatingScore: float
        public double RatingScore { get; set; } = 0;

        // DateCreated & UpdateDate
        [Description("Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Description("Ngày cập nhật chap mới nhất")]
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        // --- Foreign Keys & Relationships ---

        // AuthorId: Guid? (Liên kết tác giả - nếu có)
        public Guid? AuthorId { get; set; }
        public virtual Author? Author { get; set; }

        // Khóa ngoại đến Category (Giữ lại từ code cũ vì cấu trúc truyện thường cần danh mục)
        [Required]
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
