using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WibuHub.ApplicationCore.Interface;
using static System.Net.Mime.MediaTypeNames;

namespace WibuHub.ApplicationCore.Entities
{
    [Index(nameof(StoryId), nameof(ChapterNumber))]
    [Index(nameof(Slug), IsUnique = true)]
    public class Chapter : ISoftDelete
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid StoryId { get; set; }
        public string StoryName { get; set; } = string.Empty;

        [ForeignKey("StoryId")]
        public virtual Story? Story { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public double ChapterNumber { get; set; }

        [Required]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string Slug { get; set; } = string.Empty;

        public int ViewCount { get; set; } = 0;

        public string? Content { get; set; }

        [NotMapped]
        public List<string> ImageUrls { get; set; } = new();

        public int ServerId { get; set; } = 1;

        [Description("Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<ChapterImage> Images { get; set; } = new List<ChapterImage>();

        // ==========================================
        // --- THÊM CẤU HÌNH VIP & KIẾM TIỀN Ở ĐÂY ---
        // ==========================================

        [Description("Đánh dấu chương này khóa vĩnh viễn, chỉ dành cho VIP")]
        public bool IsPremium { get; set; } = false;

        [Description("Ngày mở khóa cho tài khoản thường (Early Access)")]
        public DateTime? UnlockDate { get; set; }

        [Description("Giá xu (Points) để mở khóa riêng chương này nếu user không mua VIP")]
        public int Price { get; set; } = 0;

        // Hàm helper không lưu vào DB, giúp code API gọn hơn rất nhiều
        [NotMapped]
        public bool IsFreeToRead => !IsPremium && (!UnlockDate.HasValue || UnlockDate.Value <= DateTime.UtcNow);
    }
}