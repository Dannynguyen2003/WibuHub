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
        [Required(ErrorMessage = "Slug không được để trống")]
        public string Slug { get; set; } = string.Empty; 

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
    }
}