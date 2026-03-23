using System.ComponentModel.DataAnnotations;
using WibuHub.ApplicationCore.Entities;
using Microsoft.AspNetCore.Http;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class ChapterDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "StoryId là bắt buộc")]
        [Display(Name = "Story ID")]
        public Guid StoryId { get; set; }

        public string StoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên chapter là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Tên chapter không được vượt quá 150 ký tự")]
        [Display(Name = "Tên chapter")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Số thứ tự")]
        public double ChapterNumber { get; set; }

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

        // ==========================================
        // --- THÊM CÁC TRƯỜNG VIP Ở ĐÂY ---
        // ==========================================

        [Display(Name = "Chương VIP (Chỉ VIP mới được đọc)")]
        public bool IsPremium { get; set; } = false;

        [Display(Name = "Ngày mở khóa cho User thường (Early Access)")]
        public DateTime? UnlockDate { get; set; }

        [Display(Name = "Giá mua lẻ (Xu)")]
        public int Price { get; set; } = 0;

        // Thuộc tính này chỉ để Read-only, giúp Frontend biết có nên hiển thị nút "Đọc ngay" hay hiện Ổ Khóa
        public bool IsFreeToRead => !IsPremium && (!UnlockDate.HasValue || UnlockDate.Value <= DateTime.UtcNow);

        // ==========================================

        // Ảnh đã lưu (để trả về)
        public List<string> ImageUrls { get; set; } = new();

        // Ảnh upload mới (dùng khi tạo/cập nhật)
        public List<IFormFile>? UploadImages { get; set; }

        // Thông tin ảnh đã lưu (nếu cần)
        public virtual ICollection<ChapterImage> Images { get; set; } = new List<ChapterImage>();
    }
}