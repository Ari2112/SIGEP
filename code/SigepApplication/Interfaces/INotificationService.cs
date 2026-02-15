using SigepApplication.DTOs.Notifications;

namespace SigepApplication.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task CreateNotificationAsync(int userId, string title, string message, string type, 
        string? module = null, string? entityType = null, int? entityId = null);
    Task NotifyRequestStatusChangeAsync(int employeeUserId, string requestType, int requestId, string newStatus, string? comments = null);
}
