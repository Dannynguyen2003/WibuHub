using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class FollowDto
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        [Display(Name = "Người dùng")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Truyện là bắt buộc")]
        [Display(Name = "Truyện")]
        public Guid StoryId { get; set; }

        [Display(Name = "Tên truyện")]
        public string? StoryTitle { get; set; }

        [Display(Name = "Ngày theo dõi")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Số chương chưa đọc")]
        public int UnreadCount { get; set; } = 0;
    }
}
