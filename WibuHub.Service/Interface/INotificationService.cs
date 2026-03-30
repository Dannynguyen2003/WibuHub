using WibuHub.ApplicationCore.DTOs.Shared;
using WibuHub.ApplicationCore.Entities;

namespace WibuHub.Service.Interface
{
    public interface INotificationService
    {
        // Lấy danh sách thông báo của 1 user (thường sẽ giới hạn số lượng để tối ưu)
        Task<List<NotificationDto>> GetByUserIdAsync(Guid userId);

        // Đánh dấu 1 thông báo cụ thể là đã đọc
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

        // Đánh dấu toàn bộ thông báo của user đó là đã đọc
        Task<bool> MarkAllAsReadAsync(Guid userId);
    }
}