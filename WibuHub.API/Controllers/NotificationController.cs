using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WibuHub.Service.Interface;
using System.Security.Claims;

namespace WibuHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Kế thừa ControllerBase cho chuẩn API
    public class NotificationController : ControllerBase
    {
        private readonly ICustomEmailSender _emailSender;
        private readonly INotificationService _notificationService; // Thêm service xử lý DB

        // Inject cả 2 service vào đây
        public NotificationController(ICustomEmailSender emailSender, INotificationService notificationService)
        {
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        // ==========================================
        // 1. THÔNG BÁO QUA EMAIL (Code cũ của bạn)
        // ==========================================
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification(string userEmail)
        {
            var subject = "Welcome to WibuHub!";
            var body = "<h1>Hello!</h1><p>This is a test email sent via MailKit.</p>";
            await _emailSender.SendEmailAsync(userEmail, subject, body);
            return Ok("Email sent successfully.");
        }

        // ==========================================
        // 2. THÔNG BÁO TRONG APP (Cho biểu tượng quả chuông)
        // ==========================================

        // Lấy danh sách thông báo của user đang đăng nhập
        [HttpGet]
        [Authorize] // Bắt buộc đăng nhập
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            var notifications = await _notificationService.GetByUserIdAsync(userId);
            return Ok(notifications);
        }

        // Đánh dấu 1 thông báo là đã đọc
        [HttpPut("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            var result = await _notificationService.MarkAsReadAsync(id, userId);

            if (result) return Ok(new { success = true });
            return BadRequest(new { success = false, message = "Không thể cập nhật thông báo." });
        }

        // Đánh dấu tất cả là đã đọc
        [HttpPut("read-all")]
        [Authorize]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new { success = true });
        }
    }
}