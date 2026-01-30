using System.ComponentModel.DataAnnotations;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class CategoryDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Vị trí")]
        public int Position { get; set; }
    }
}
