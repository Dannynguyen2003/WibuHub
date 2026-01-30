using Microsoft.AspNetCore.Http; // Cần cái này để nhận file upload
using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class ChapterDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề chapter không được để trống")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Số thứ tự")]
        public double ChapterNumber { get; set; } // Dùng double để hỗ trợ chap 1.5

        public Guid StoryId { get; set; }

        // --- Phần hiển thị (Output) ---
        public string? StoryTitle { get; set; } // Để hiện tên truyện
        public List<string> ImageUrls { get; set; } = new List<string>(); // Danh sách link ảnh đã lưu

        // --- Phần nhập liệu (Input) ---
        // Dùng để upload ảnh lúc Create/Update
        [Display(Name = "Danh sách ảnh truyện")]
        public List<IFormFile>? Pages { get; set; }
    }
}