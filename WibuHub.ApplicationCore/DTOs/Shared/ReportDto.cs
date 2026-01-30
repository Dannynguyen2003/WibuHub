using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class ReportDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Truyện là bắt buộc")]
        [Display(Name = "Truyện")]
        public Guid StoryId { get; set; }

        [Display(Name = "Tên truyện")]
        public string? StoryTitle { get; set; }

        [Display(Name = "Chương")]
        public Guid? ChapterId { get; set; }

        [Display(Name = "Tên chương")]
        public string? ChapterName { get; set; }

        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Nội dung không được vượt quá 255 ký tự")]
        [Display(Name = "Nội dung báo lỗi")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public int Status { get; set; } = 0;

        [Display(Name = "Ngày tạo")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
