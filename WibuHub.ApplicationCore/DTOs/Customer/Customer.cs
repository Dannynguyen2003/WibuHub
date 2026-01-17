using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.ApplicationCore.DTOs.Customer
{
    public class Customer
    {
        public class UpdateRoleDto
        {
            [StringLength(50, ErrorMessage = "Nickname không được quá 50 ký tự")]
            public string? Nickname { get; set; }
            public string? RoleName { get; set; } // Tên Role muốn gán
            public string? Fullname { get; set; }

            public string? Avatar { get; set; } // URL ảnh
        }

        // 2. DTO dùng để Theo dõi / Bỏ theo dõi truyện (POST)
        public class ToggleFollowDto
        {
            [Required]
            public int StoryId { get; set; }
        }

        // 3. DTO dùng để Lưu tiến độ đọc (POST)
        public class ReadingProgressDto
        {
            [Required]
            public int StoryId { get; set; }

            [Required]
            public int ChapterId { get; set; }
        }

        // 4. DTO dùng để Mua Chapter (POST)
        public class BuyChapterDto
        {
            [Required]
            public int ChapterId { get; set; }
        }

        // 5. DTO dùng để Thêm vào giỏ hàng (POST)
        public class AddToCartDto
        {
            [Required]
            public int ChapterId { get; set; }
        }
    }
}
