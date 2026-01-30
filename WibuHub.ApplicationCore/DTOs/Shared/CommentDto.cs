using System.ComponentModel.DataAnnotations;
namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class CommentDto
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
        [Display(Name = "Chương")]
        public Guid? ChapterId { get; set; }
        [Display(Name = "Tên chương")]
        public string? ChapterName { get; set; }
        [Required(ErrorMessage = "Nội dung là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;
        [Display(Name = "Bình luận cha")]
        public Guid? ParentId { get; set; }
        [Display(Name = "Ngày tạo")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        [Display(Name = "Lượt thích")]
        public int LikeCount { get; set; } = 0;
    }
}