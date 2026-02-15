namespace SigepApplication.Interfaces;

public interface IAuditService
{
    Task LogAsync(int userId, string action, string module, string? entityType = null, 
        int? entityId = null, object? oldValues = null, object? newValues = null, 
        string? description = null, string? ipAddress = null, string? userAgent = null);
    
    Task LogLoginAsync(int userId, string ipAddress, string? userAgent);
    Task LogLogoutAsync(int userId, string? ipAddress, string? userAgent);
}
