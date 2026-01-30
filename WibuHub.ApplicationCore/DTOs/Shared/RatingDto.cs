using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class RatingDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID là bắt buộc")]
        [Display(Name = "Người dùng")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Truyện là bắt buộc")]
        [Display(Name = "Truyện")]
        public Guid StoryId { get; set; }

        [Display(Name = "Tên truyện")]
        public string? StoryTitle { get; set; }

        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5")]
        [Display(Name = "Điểm đánh giá")]
        public int Score { get; set; }

        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
