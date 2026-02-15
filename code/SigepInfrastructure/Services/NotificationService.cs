using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Notifications;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                Module = n.Module,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(int userId, string title, string message, string type, 
        string? module = null, string? entityType = null, int? entityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Module = module,
            EntityType = entityType,
            EntityId = entityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task NotifyRequestStatusChangeAsync(int employeeUserId, string requestType, int requestId, string newStatus, string? comments = null)
    {
        string title = $"Solicitud de {requestType} {newStatus}";
        string message = newStatus switch
        {
            "Aprobada" => $"Tu solicitud de {requestType.ToLower()} ha sido aprobada.",
            "Rechazada" => $"Tu solicitud de {requestType.ToLower()} ha sido rechazada." + 
                          (string.IsNullOrEmpty(comments) ? "" : $" Motivo: {comments}"),
            _ => $"El estado de tu solicitud de {requestType.ToLower()} ha cambiado a: {newStatus}"
        };

        string type = newStatus == "Aprobada" ? "SUCCESS" : newStatus == "Rechazada" ? "WARNING" : "INFO";

        await CreateNotificationAsync(employeeUserId, title, message, type, requestType.ToUpper(), requestType, requestId);
    }
}
