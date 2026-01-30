using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class NotificationDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID là bắt buộc")]
        [Display(Name = "Người dùng")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Tiêu đề không được vượt quá 150 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [MaxLength(500, ErrorMessage = "Nội dung không được vượt quá 500 ký tự")]
        [Display(Name = "Nội dung")]
        public string Message { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "URL không được vượt quá 255 ký tự")]
        [Display(Name = "URL đích")]
        public string? TargetUrl { get; set; }

        [Display(Name = "Đã đọc")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
