using System.ComponentModel.DataAnnotations;

namespace WibuHub.API.Models
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; init; }

        [Required]
        public string ConfirmPassword { get; init; }

        public string? ReturnUrl { get; init; }
    }
}
