using Microsoft.AspNetCore.Identity;

namespace WibuHub.ApplicationCore.Entities
{
    // Kế thừa IdentityUser (dùng Guid cho đồng bộ với code cũ của bạn)
    public class StoryUser : IdentityUser
    {
        public string? FullName { get; set; } // [cite: 64]
        public string? Avatar { get; set; }   // [cite: 65]

        // Các quan hệ cũ của bạn (giữ nguyên)
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Follow> Follows { get; set; } = new List<Follow>();
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<History> Histories { get; set; } = new List<History>();
    }
}