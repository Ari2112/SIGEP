using Microsoft.EntityFrameworkCore;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;
using System.Text.Json;

namespace SigepInfrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int userId, string action, string module, string? entityType = null, 
        int? entityId = null, object? oldValues = null, object? newValues = null, 
        string? description = null, string? ipAddress = null, string? userAgent = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            Module = module,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogLoginAsync(int userId, string ipAddress, string? userAgent)
    {
        await LogAsync(userId, "LOGIN", "AUTH", "User", userId, 
            description: "Inicio de sesión", ipAddress: ipAddress, userAgent: userAgent);
        
        // Actualizar último login del usuario
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task LogLogoutAsync(int userId, string? ipAddress, string? userAgent)
    {
        await LogAsync(userId, "LOGOUT", "AUTH", "User", userId, 
            description: "Cierre de sesión", ipAddress: ipAddress, userAgent: userAgent);
    }
}
