using Microsoft.AspNetCore.Identity;

namespace WibuHub.ApplicationCore.Entities.Identity
{
    // Kế thừa IdentityUser (dùng Guid cho đồng bộ với code cũ của bạn)
    public class StoryUser : IdentityUser
    {
        public string Fullname { get; set; }
        public string AvatarUrl { get; set; }
        public decimal WalletBalance { get; set; } // Số dư thật
        public DateTime JoinedDate { get; set; }

    }
}