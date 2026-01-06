using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // UserId: Guid (Người bình luận)
        // Bắt buộc (Required) theo yêu cầu của bạn
        [Required]
        public Guid UserId { get; set; }
        // public virtual AppUser User { get; set; } // Uncomment dòng này nếu bạn đã có class User

        // ComicId: Guid (Bình luận ở truyện nào)
        // Map ComicId khóa ngoại trỏ tới bảng Story
        [Required]
        public Guid StoryId { get; set; }

        [ForeignKey("StoryId")]
        public virtual Story Story { get; set; } = null!;

        // ChapterId: Guid? (Bình luận cụ thể ở chap nào - có thể null)
        // Nếu null => Bình luận chung cho cả bộ truyện
        // Nếu có value => Bình luận riêng cho Chapter đó
        public Guid? ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter? Chapter { get; set; }

        // Content: nvarchar(1000)
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        // ParentId: Guid? (Trả lời cho bình luận nào - đệ quy)
        public Guid? ParentId { get; set; }

        // Navigation Property trỏ về comment cha
        [ForeignKey("ParentId")]
        public virtual Comment? Parent { get; set; }

        // Navigation Property danh sách các câu trả lời (Replies) của comment này
        // Giúp dễ dàng lấy danh sách: comment.Replies
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        // CreateDate: DateTime
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // LikeCount: int
        public int LikeCount { get; set; } = 0;
    }
}