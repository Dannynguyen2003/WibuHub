using System.ComponentModel.DataAnnotations;
namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class AuthorDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Tên tác giả là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Tên tác giả không được vượt quá 150 ký tự")]
        [Display(Name = "Tên tác giả")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Slug")]
        [Required(ErrorMessage = "Slug là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Slug không được vượt quá 150 ký tự")]
        public string Slug { get; set; } = string.Empty;
    }
}
