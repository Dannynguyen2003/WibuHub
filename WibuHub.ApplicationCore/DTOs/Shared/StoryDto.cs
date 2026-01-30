using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class StoryDto
    {
        public Guid Id { get; set; }

        [Display(Name = "Tên truyện")]
        [Required(ErrorMessage = "Tên truyện không được để trống")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Slug")]
        [Required(ErrorMessage = "Slug là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Slug không được vượt quá 150 ký tự")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "Tác giả")]
        public string? AuthorName { get; set; }

        [Display(Name = "Ảnh bìa")]
        public string? CoverImage { get; set; }
        [Display(Name = "Trạng thái")]
        public string? Status { get; set; }
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Dropdown chọn Danh mục (Category)
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public Guid CategoryId { get; set; }

        // Để hiển thị tên danh mục trong bảng (chỉ đọc)
        public string? CategoryName { get; set; }
    }
}