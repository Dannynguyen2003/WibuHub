using System.ComponentModel.DataAnnotations;

namespace WibuHub.ViewModels
{
    public class AdminProfileVM
    {
        [Display(Name = "H? và tên")]
        public string? FullName { get; set; }

        [Display(Name = "S? ði?n tho?i")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "?nh ð?i di?n")]
        [Url]
        public string? Avatar { get; set; }

        public string? Email { get; set; }

        public string? UserName { get; set; }
    }
}
