using System.ComponentModel.DataAnnotations;

namespace WibuHub.MVC.ViewModels
{
    public class RoleVM
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên role là bắt buộc")]
        [Display(Name = "Tên Role")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Số người dùng")]
        public int UserCount { get; set; }
    }

    public class CreateRoleVM
    {
        [Required(ErrorMessage = "Tên role là bắt buộc")]
        [Display(Name = "Tên Role")]
        [StringLength(256, ErrorMessage = "Tên role không được vượt quá 256 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }
    }

    public class EditRoleVM
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên role là bắt buộc")]
        [Display(Name = "Tên Role")]
        [StringLength(256, ErrorMessage = "Tên role không được vượt quá 256 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }
    }

    public class RoleDetailsVM
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserCount { get; set; }
        public List<UserInRoleVM> Users { get; set; } = new();
    }

    public class UserInRoleVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }
}
