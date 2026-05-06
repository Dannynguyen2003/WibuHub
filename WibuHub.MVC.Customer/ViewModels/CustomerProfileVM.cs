using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WibuHub.MVC.Customer.ViewModels
{
    public class CustomerProfileVM
    {
        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        [Display(Name = "Phone number")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Avatar")]
        public IFormFile? AvatarFile { get; set; }

        public string? Avatar { get; set; }

        public string? Email { get; set; }

        public string? UserName { get; set; }

        // --- Các trường dữ liệu Level ---
        public int Level { get; set; }
        public int Points { get; set; }
        public int ExpPercentage { get; set; }
        public int CurrentExperience { get; set; }
        public int ExpPerLevel { get; set; } = 100;

        public List<HistoryItemVM> ReadingHistories { get; set; } = new List<HistoryItemVM>();
        public List<FollowItemVM> FollowedStories { get; set; } = new List<FollowItemVM>();

        // Hàm tự động xếp loại cảnh giới dựa vào phần trăm kinh nghiệm
        public string RankTier
        {
            get
            {
                if (ExpPercentage < 30) return "Sơ Kỳ";
                if (ExpPercentage < 60) return "Trung Kỳ";
                if (ExpPercentage < 90) return "Hậu Kỳ";
                return "Viên Mãn";
            }
        }
    }
}
