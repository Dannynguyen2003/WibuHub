using Microsoft.AspNetCore.Identity;

namespace WibuHub.ApplicationCore.Entities
{
    // Kế thừa IdentityUser (dùng Guid cho đồng bộ với code cũ của bạn)
    public class StoryUser : IdentityUser
    {
        public string? FullName { get; set; } // [cite: 64]
        public string? Avatar { get; set; }   // [cite: 65]

        // Các quan hệ cũ của bạn (giữ nguyên)
        
    }
}