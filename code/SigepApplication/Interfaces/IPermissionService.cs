using SigepApplication.DTOs.Permissions;

namespace SigepApplication.Interfaces;

public interface IPermissionService
{
    // === TIPOS DE PERMISOS ===
    Task<IEnumerable<PermissionTypeDto>> GetPermissionTypesAsync();
    Task<PermissionTypeDto?> GetPermissionTypeByIdAsync(int typeId);

    // === SOLICITUDES DE PERMISOS ===
    Task<PermissionRequestDto?> GetRequestByIdAsync(int requestId);
    Task<IEnumerable<PermissionRequestDto>> GetEmployeeRequestsAsync(int employeeId);
    Task<IEnumerable<PermissionRequestDto>> GetPendingRequestsAsync();
    Task<IEnumerable<PermissionRequestDto>> GetAllRequestsAsync(PermissionRequestFilterDto? filter = null);

    // === ACCIONES DE SOLICITUD ===
    Task<PermissionRequestDto> CreateRequestAsync(CreatePermissionRequestDto dto, int userId);
    Task<PermissionRequestDto> ApproveRequestAsync(int requestId, int approverUserId, string? comments = null);
    Task<PermissionRequestDto> RejectRequestAsync(int requestId, int approverUserId, string reason);
    Task<PermissionRequestDto> CancelRequestAsync(int requestId, int userId, string? reason = null);

    // === ESTADÍSTICAS ===
    Task<PermissionUsageSummaryDto> GetUsageSummaryAsync(int employeeId, int year);
}
