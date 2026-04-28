using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WibuHub.Service.Interface;

namespace WibuHub.MVC.Customer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notification")] // Consider changing to "api/notifications"
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            var notifications = await _notificationService.GetByUserIdAsync(userId);
            return Ok(notifications);
        }

        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            var updated = await _notificationService.MarkAsReadAsync(id, userId);
            if (!updated)
            {
                return BadRequest(new { success = false, message = "Failed to update the notification." });
            }

            return Ok(new { success = true });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = true });
        }

        [HttpDelete("read")]
        public async Task<IActionResult> DeleteReadNotifications()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(new { message = "Unable to identify user." });
            }

            var deletedCount = await _notificationService.DeleteReadAsync(userId);
            return Ok(new { success = true, deletedCount });
        }

        // --- Helper Methods ---

        private bool TryGetUserId(out Guid userId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out userId);
        }
    }
}