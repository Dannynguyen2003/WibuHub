using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? TargetUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreateDate { get; set; }

        // (Tùy chọn) Thêm một trường để frontend dễ hiển thị "5 phút trước", "1 giờ trước"
        public string? TimeAgo { get; set; }
    }
}
