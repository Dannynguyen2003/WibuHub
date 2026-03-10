using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WibuHub.ViewModels
{
    public class AdminProfileVM
    {
        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        [Display(Name = "Phone number")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Avatar")]
        public IFormFile? AvatarFile { get; set; }

        public string? Avatar { get; set; }

        public string? Email { get; set; }

        public string? UserName { get; set; }
    }
}
