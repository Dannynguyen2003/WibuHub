using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // UserId: Guid (Thông báo của ai)
        [Required]
        public Guid UserId { get; set; }
        // public virtual AppUser User { get; set; } // Uncomment nếu đã có class User

        // Title: nvarchar(150)
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        // Message: nvarchar(500)
        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        // TargetUrl: varchar(255) (Link đích khi click vào)
        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? TargetUrl { get; set; }

        // IsRead: bit (Trạng thái đã xem)
        public bool IsRead { get; set; } = false;

        // CreateDate: DateTime
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}