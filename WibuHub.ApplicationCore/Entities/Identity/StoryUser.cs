using Microsoft.AspNetCore.Identity;

namespace WibuHub.ApplicationCore.Entities.Identity
{
    // Kế thừa StoryUser (dùng Guid cho đồng bộ với code cũ của bạn)
    public class StoryUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string UserType { get; set; } = "Customer"; // Admin or Customer
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public string PasswordSalt { get; set; }
        //public decimal WalletBalance { get; set; } // Số dư thật
        //public DateTime JoinedDate { get; set; }

    }
}