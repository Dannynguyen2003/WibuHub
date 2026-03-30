using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.DTOs.Shared; // Nhớ using namespace chứa NotificationDto
using WibuHub.ApplicationCore.Entities;
using WibuHub.DataLayer;
using WibuHub.Service.Interface;

namespace WibuHub.Service.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly StoryDbContext _context;

        public NotificationService(StoryDbContext context)
        {
            _context = context;
        }

        // ĐỔI KIỂU TRẢ VỀ THÀNH List<NotificationDto>
        public async Task<List<NotificationDto>> GetByUserIdAsync(Guid userId)
        {
            // 1. Kéo dữ liệu từ Database lên RAM trước
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreateDate)
                .Take(20)
                .ToListAsync();

            // 2. Map sang DTO và gọi hàm CalculateTimeAgo bằng C#
            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                TargetUrl = n.TargetUrl,
                IsRead = n.IsRead,
                CreateDate = n.CreateDate,
                TimeAgo = CalculateTimeAgo(n.CreateDate) // Gọi hàm tại đây
            }).ToList();
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var updatedRows = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));

            return true;
        }

        // Hàm helper giữ nguyên
        private static string CalculateTimeAgo(DateTime createDate)
        {
            TimeSpan timeSince = DateTime.UtcNow - createDate;

            if (timeSince.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSince.TotalHours < 1)
                return $"{(int)timeSince.TotalMinutes} phút trước";
            if (timeSince.TotalDays < 1)
                return $"{(int)timeSince.TotalHours} giờ trước";
            if (timeSince.TotalDays < 7)
                return $"{(int)timeSince.TotalDays} ngày trước";
            if (timeSince.TotalDays < 30)
                return $"{(int)(timeSince.TotalDays / 7)} tuần trước";
            if (timeSince.TotalDays < 365)
                return $"{(int)(timeSince.TotalDays / 30)} tháng trước";

            return $"{(int)(timeSince.TotalDays / 365)} năm trước";
        }
    }
}