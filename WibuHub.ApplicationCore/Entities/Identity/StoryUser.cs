using Microsoft.AspNetCore.Identity;

namespace WibuHub.ApplicationCore.Entities.Identity
{
    // Kế thừa IdentityUser (dùng string theo cấu hình IdentityDbContext)
    public class StoryUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Avatar { get; set; }

        // --- Thêm các cột cho hệ thống Cấp độ và Tích điểm ---
        public int Level { get; set; } = 0;       // Cấp độ hiện tại
        public int Experience { get; set; } = 0;  // Kinh nghiệm đang có ở cấp hiện tại
        public int Points { get; set; } = 0;      // Điểm tích lũy (Xu) dùng để giảm giá
    }
}