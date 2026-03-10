namespace WibuHub.API.Models
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string[] Roles { get; set; } = [];
    }
}
