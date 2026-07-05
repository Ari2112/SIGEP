using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SigepApplication.DTOs.Notifications;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
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

        // Además de la notificación interna, se envía un correo al usuario.
        // Si el envío falla (SMTP no configurado, sin internet, etc.) queda
        // registrado en el log pero no interrumpe el flujo que la disparó.
        await SendNotificationEmailAsync(userId, title, message);
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

    private async Task SendNotificationEmailAsync(int userId, string title, string message)
    {
        try
        {
            var email = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("No se envió correo de notificación: el usuario {UserId} no tiene correo registrado.", userId);
                return;
            }

            var htmlBody = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                    <h2 style=""color: #2E4057;"">{title}</h2>
                    <p style=""font-size: 15px; color: #333;"">{message}</p>
                    <hr style=""border: none; border-top: 1px solid #ddd; margin: 20px 0;"" />
                    <p style=""font-size: 12px; color: #888;"">
                        Este es un mensaje automático del Sistema Integral de Gestión de Personal (SIGEP) —
                        Centro Agrícola Cantonal de Coronado. Por favor no respondas este correo.
                    </p>
                </div>";

            await _emailService.SendEmailAsync(email, title, htmlBody);
        }
        catch (Exception ex)
        {
            // No debe romper la creación de la notificación si algo falla al buscar el correo.
            _logger.LogError(ex, "Error inesperado al intentar enviar el correo de notificación para el usuario {UserId}.", userId);
        }
    }
}