using System.ComponentModel.DataAnnotations;
namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class ChapterDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Truyện là bắt buộc")]
        [Display(Name = "Truyện")]
        public Guid StoryId { get; set; }
        [Display(Name = "Tên truyện")]
        public string? StoryTitle { get; set; }
        [Required(ErrorMessage = "Tên chương là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Tên chương không được vượt quá 150 ký tự")]
        [Display(Name = "Tên chương")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Số chương là bắt buộc")]
        [Display(Name = "Số chương")]
        public double Number { get; set; }
        [Required(ErrorMessage = "Slug là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Slug không được vượt quá 150 ký tự")]
        [Display(Name = "Slug")]
        public string Slug { get; set; } = string.Empty;
        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; } = 0;
        [Display(Name = "Nội dung")]
        public string? Content { get; set; }
        [Display(Name = "Server ID")]
        public int ServerId { get; set; } = 1;
        [Display(Name = "Ngày tạo")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        [Display(Name = "Giá")]
        public decimal Price { get; set; } = 0;
        [Display(Name = "Giảm giá")]
        public decimal Discount { get; set; } = 0;
    }
}