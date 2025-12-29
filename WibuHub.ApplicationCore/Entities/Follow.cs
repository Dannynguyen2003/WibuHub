using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class Follow
    {
        // UserId: Guid
        // Khóa ngoại trỏ đến bảng User
        public Guid UserId { get; set; }
        // public virtual AppUser User { get; set; } = null!; // Uncomment nếu bạn đã có class User

        // ComicId: Guid
        // Khóa ngoại trỏ đến bảng Story (Comic)
        public Guid ComicId { get; set; }

        [ForeignKey("ComicId")]
        public virtual Story Story { get; set; } = null!;

        // CreateDate: DateTime (Ngày bắt đầu theo dõi)
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // UnreadCount: int (Số chap chưa đọc)
        public int UnreadCount { get; set; } = 0;
    }
}