using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WibuHub.ApplicationCore.Entities.Identity
{
    public class StoryUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Avatar { get; set; }

        // --- Hệ thống Cấp độ và Tích điểm ---
        public int Level { get; set; } = 0;
        public int Experience { get; set; } = 0;
        public int Points { get; set; } = 0;

        // --- ĐẶC QUYỀN VIP ---
        public DateTime? VipExpireDate { get; set; } // Thời điểm hết hạn VIP (Lưu UTC)

        // Property phụ trợ giúp check nhanh VIP (Không lưu vào Database)
        [NotMapped]
        public bool IsVip => VipExpireDate.HasValue && VipExpireDate.Value > DateTime.UtcNow;
    }
}