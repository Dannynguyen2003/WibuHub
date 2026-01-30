using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class StoryDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Tên thay thế không được vượt quá 500 ký tự")]
        [Display(Name = "Tên thay thế")]
        public string? AlternativeName { get; set; }
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
        [MaxLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự")]
        [Display(Name = "Hình ảnh")]
        public string? Thumbnail { get; set; }
        [Display(Name = "Trạng thái")]
        [Description("0: Đang tiến hành, 1: Hoàn thành, 2: Tạm ngưng")]
        public int Status { get; set; } = 0;
        [Display(Name = "Lượt xem")]
        public long ViewCount { get; set; } = 0;
        [Display(Name = "Người theo dõi")]
        public int FollowCount { get; set; } = 0;
        [Display(Name = "Điểm đánh giá")]
        public double RatingScore { get; set; } = 0;
        [Display(Name = "Ngày tạo")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        [Display(Name = "Tác giả")]
        public Guid? AuthorId { get; set; }
        [Display(Name = "Tên tác giả")]
        public string? AuthorName { get; set; }
        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [Display(Name = "Danh mục")]
        public Guid CategoryId { get; set; }
        [Display(Name = "Tên danh mục")]
        public string? CategoryName { get; set; }
    }
}