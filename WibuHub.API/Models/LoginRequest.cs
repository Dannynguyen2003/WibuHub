using System.ComponentModel.DataAnnotations;

namespace WibuHub.API.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc username")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}