using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Notifications;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Obtiene las notificaciones del usuario actual
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(GetUserId(), unreadOnly);
        return Ok(notifications);
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(GetUserId());
        return Ok(new { count });
    }

    /// <summary>
    /// Marca una notificación como leída
    /// </summary>
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id, GetUserId());
        return NoContent();
    }

    /// <summary>
    /// Marca todas las notificaciones como leídas
    /// </summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(GetUserId());
        return NoContent();
    }
}
