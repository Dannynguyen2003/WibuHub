using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class CategoryDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên quá dài")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Slug")]
        [Required(ErrorMessage = "Slug là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Slug không được vượt quá 150 ký tự")]
        public string Slug { get; set; } = string.Empty;
    }
}