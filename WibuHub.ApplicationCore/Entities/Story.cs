using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WibuHub.ApplicationCore.Interface;

namespace WibuHub.ApplicationCore.Entities
{
    public class Story : ISoftDelete
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Title: nvarchar(255)
        [Required]
        [MaxLength(255)]
        public string StoryName { get; set; } = string.Empty;

        // AlternativeName: nvarchar(500)
        [MaxLength(500)]
        public string? AlternativeName { get; set; }

        // Description: nvarchar(MAX)
        // Trong EF Core, string không giới hạn MaxLength mặc định sẽ là nvarchar(MAX)
        public string? Description { get; set; }
        
 

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

        // Thumbnail
        [MaxLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? CoverImage { get; set; }

        // DateCreated & UpdateDate
        [Description("Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Description("Ngày cập nhật chap mới nhất")]
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        // --- Foreign Keys & Relationships ---

        // AuthorId: Guid? (Liên kết tác giả - nếu có)
        public Guid? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public virtual Author? Author { get; set; }

        // Khóa ngoại đến Category (Giữ lại từ code cũ vì cấu trúc truyện thường cần danh mục)
        [Required]
        public Guid CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }


        // 1 Story có nhiều Chapter
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

        // 1 Story có nhiều Comment (Giữ lại từ code cũ)
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Many-to-Many: Thể loại truyện (Fixed: use ComicGenre)
        public virtual ICollection<StoryCategory> StoryCategories { get; set; } = new List<StoryCategory>();
    }

    // Helper Enum để quản lý trạng thái dễ dàng hơn trong code
    public enum StoryStatus
    {
        Ongoing = 0,    // Đang tiến hành
        Completed = 1,  // Hoàn thành
        Paused = 2      // Tạm ngưng
    }
}