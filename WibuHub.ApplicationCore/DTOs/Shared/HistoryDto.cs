using System.ComponentModel.DataAnnotations;
namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class HistoryDto
    {
        public Guid Id { get; set; }
        [Display(Name = "Người dùng")]
        public Guid? UserId { get; set; }
        [MaxLength(100, ErrorMessage = "Device ID không được vượt quá 100 ký tự")]
        [Display(Name = "Device ID")]
        public string? DeviceId { get; set; }
        [Required(ErrorMessage = "Truyện là bắt buộc")]
        [Display(Name = "Truyện")]
        public Guid StoryId { get; set; }
        [Display(Name = "Tên truyện")]
        public string? StoryTitle { get; set; }
        [Required(ErrorMessage = "Chương là bắt buộc")]
        [Display(Name = "Chương")]
        public Guid ChapterId { get; set; }
        [Display(Name = "Tên chương")]
        public string? ChapterName { get; set; }
        [Display(Name = "Thời gian đọc")]
        public DateTime ReadTime { get; set; } = DateTime.UtcNow;
    }
}