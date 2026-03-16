namespace WibuHub.API.Models
{
    public class RegisterResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? UserId { get; set; }

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string[] Roles { get; set; } = [];
    }
}