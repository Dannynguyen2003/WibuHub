using System.ComponentModel.DataAnnotations;

namespace Class.Entities
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }

        // Khóa ngoại tới người dùng
        public Guid? UserId { get; set; }

        // Khóa ngoại tới bài viết hoặc truyện
        public Guid? StoryId { get; set; }

        // Navigation property (tùy chọn)
        // public virtual User? User { get; set; }
        // public virtual Story? Story { get; set; }
    }
}