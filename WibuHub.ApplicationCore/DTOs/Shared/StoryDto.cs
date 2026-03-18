using System.ComponentModel;
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
        // Tổng số lượng chương hiện có của truyện
        [Description("Tổng số chương")]
        public int TotalChapters { get; set; } = 0;
        // Lưu tên chương mới nhất để tiện hiển thị ngoài trang chủ (VD: "Chapter 120", "Chap 50.5")
        [Description("Tên chương mới nhất")]
        [MaxLength(50)]
        public string? LatestChapter { get; set; }
        public string? TimeAgo { get; set; }
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }
        [Display(Name = "Giảm giá")]
        public decimal Discount { get; set; }
        [Display(Name = "Trạng thái")]
        public int Status { get; set; }
        public long ViewCount { get; set; } = 0;
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