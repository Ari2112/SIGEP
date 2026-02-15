using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Permissions;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepDomain.Enums;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public PermissionService(ApplicationDbContext context, INotificationService notificationService, IAuditService auditService)
    {
        _context = context;
        _notificationService = notificationService;
        _auditService = auditService;
    }

    // === TIPOS DE PERMISOS ===

    public async Task<IEnumerable<PermissionTypeDto>> GetPermissionTypesAsync()
    {
        return await _context.PermissionTypes
            .Where(pt => pt.IsActive)
            .OrderBy(pt => pt.Name)
            .Select(pt => new PermissionTypeDto
            {
                Id = pt.Id,
                Name = pt.Name,
                Description = pt.Description,
                MaxDaysPerYear = pt.MaxDaysPerYear,
                RequiresDocument = pt.RequiresDocument,
                IsPaid = pt.IsPaid,
                IsActive = pt.IsActive
            })
            .ToListAsync();
    }

    public async Task<PermissionTypeDto?> GetPermissionTypeByIdAsync(int typeId)
    {
        var pt = await _context.PermissionTypes.FindAsync(typeId);
        if (pt == null) return null;

        return new PermissionTypeDto
        {
            Id = pt.Id,
            Name = pt.Name,
            Description = pt.Description,
            MaxDaysPerYear = pt.MaxDaysPerYear,
            RequiresDocument = pt.RequiresDocument,
            IsPaid = pt.IsPaid,
            IsActive = pt.IsActive
        };
    }

    // === SOLICITUDES DE PERMISOS ===

    public async Task<PermissionRequestDto?> GetRequestByIdAsync(int requestId)
    {
        var request = await _context.PermissionRequests
            .Include(pr => pr.Employee)
            .Include(pr => pr.PermissionType)
            .Include(pr => pr.ApprovedByUser)
            .FirstOrDefaultAsync(pr => pr.Id == requestId);

        if (request == null)
            return null;

        return MapToDto(request);
    }

    public async Task<IEnumerable<PermissionRequestDto>> GetEmployeeRequestsAsync(int employeeId)
    {
        return await _context.PermissionRequests
            .Include(pr => pr.Employee)
            .Include(pr => pr.PermissionType)
            .Include(pr => pr.ApprovedByUser)
            .Where(pr => pr.EmployeeId == employeeId)
            .OrderByDescending(pr => pr.CreatedAt)
            .Select(pr => MapToDto(pr))
            .ToListAsync();
    }

    public async Task<IEnumerable<PermissionRequestDto>> GetPendingRequestsAsync()
    {
        return await _context.PermissionRequests
            .Include(pr => pr.Employee)
            .Include(pr => pr.PermissionType)
            .Where(pr => pr.Status == RequestStatus.Pendiente || pr.Status == RequestStatus.EnRevision)
            .OrderBy(pr => pr.CreatedAt)
            .Select(pr => MapToDto(pr))
            .ToListAsync();
    }

    public async Task<IEnumerable<PermissionRequestDto>> GetAllRequestsAsync(PermissionRequestFilterDto? filter = null)
    {
        var query = _context.PermissionRequests
            .Include(pr => pr.Employee)
            .Include(pr => pr.PermissionType)
            .Include(pr => pr.ApprovedByUser)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
                query = query.Where(pr => pr.EmployeeId == filter.EmployeeId.Value);
            
            if (filter.PermissionTypeId.HasValue)
                query = query.Where(pr => pr.PermissionTypeId == filter.PermissionTypeId.Value);
            
            if (filter.Status.HasValue)
                query = query.Where(pr => pr.Status == filter.Status.Value);
            
            if (filter.DateFrom.HasValue)
                query = query.Where(pr => pr.StartDate >= filter.DateFrom.Value);
            
            if (filter.DateTo.HasValue)
                query = query.Where(pr => pr.StartDate <= filter.DateTo.Value);
        }

        return await query
            .OrderByDescending(pr => pr.CreatedAt)
            .Select(pr => MapToDto(pr))
            .ToListAsync();
    }

    public async Task<PermissionRequestDto> CreateRequestAsync(CreatePermissionRequestDto dto, int userId)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");

        var permissionType = await _context.PermissionTypes.FindAsync(dto.PermissionTypeId);
        if (permissionType == null)
            throw new ArgumentException("Tipo de permiso no encontrado");

        if (!permissionType.IsActive)
            throw new InvalidOperationException("El tipo de permiso no está activo");

        // Validar fechas
        if (dto.StartDate < DateTime.Today)
            throw new InvalidOperationException("La fecha de inicio no puede ser en el pasado");

        // Calcular duración
        decimal durationDays = 0;
        if (dto.IsPartialDay)
        {
            // Permiso parcial: calcular fracción del día
            if (!dto.StartTime.HasValue || !dto.EndTime.HasValue)
                throw new InvalidOperationException("Para permisos parciales debe especificar hora de inicio y fin");
            
            var duration = dto.EndTime.Value - dto.StartTime.Value;
            durationDays = (decimal)duration.TotalHours / 8; // Asumiendo jornada de 8 horas
        }
        else
        {
            // Permiso de día completo
            durationDays = (decimal)(dto.EndDate ?? dto.StartDate).Subtract(dto.StartDate).TotalDays + 1;
        }

        // Verificar límite anual del tipo de permiso
        if (permissionType.MaxDaysPerYear.HasValue)
        {
            var usedThisYear = await _context.PermissionRequests
                .Where(pr => pr.EmployeeId == dto.EmployeeId 
                    && pr.PermissionTypeId == dto.PermissionTypeId
                    && pr.StartDate.Year == dto.StartDate.Year
                    && (pr.Status == RequestStatus.Aprobada || pr.Status == RequestStatus.Pendiente))
                .SumAsync(pr => pr.DurationDays);

            if (usedThisYear + durationDays > permissionType.MaxDaysPerYear.Value)
                throw new InvalidOperationException(
                    $"Ha excedido el límite anual de {permissionType.MaxDaysPerYear} días para este tipo de permiso. " +
                    $"Usado: {usedThisYear}, Solicitado: {durationDays}");
        }

        // Verificar solapamiento
        var overlappingRequest = await _context.PermissionRequests
            .Where(pr => pr.EmployeeId == dto.EmployeeId 
                && (pr.Status == RequestStatus.Pendiente || pr.Status == RequestStatus.Aprobada)
                && pr.StartDate == dto.StartDate
                && pr.IsPartialDay == dto.IsPartialDay)
            .FirstOrDefaultAsync();

        if (overlappingRequest != null && !dto.IsPartialDay)
            throw new InvalidOperationException("Ya existe una solicitud de permiso para la misma fecha");

        // Validar documento si es requerido
        if (permissionType.RequiresDocument && string.IsNullOrEmpty(dto.DocumentUrl))
            throw new InvalidOperationException($"El tipo de permiso '{permissionType.Name}' requiere documento adjunto");

        var request = new PermissionRequest
        {
            EmployeeId = dto.EmployeeId,
            PermissionTypeId = dto.PermissionTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate ?? dto.StartDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            IsPartialDay = dto.IsPartialDay,
            DurationDays = durationDays,
            Reason = dto.Reason,
            DocumentUrl = dto.DocumentUrl,
            Status = RequestStatus.Pendiente,
            CreatedAt = DateTime.UtcNow
        };

        _context.PermissionRequests.Add(request);
        await _context.SaveChangesAsync();

        // Auditoría
        await _auditService.LogAsync(userId, "CREATE", "PERMISOS", "PermissionRequest", request.Id,
            newValues: new { request.PermissionTypeId, request.StartDate, request.DurationDays },
            description: $"Nueva solicitud de permiso: {permissionType.Name}");

        // Notificar al supervisor
        var supervisorUser = await GetSupervisorUserAsync(employee.Id);
        if (supervisorUser != null)
        {
            await _notificationService.CreateNotificationAsync(
                supervisorUser.Id,
                "Nueva solicitud de permiso",
                $"{employee.FullName} ha solicitado permiso de tipo '{permissionType.Name}' para el {dto.StartDate:dd/MM/yyyy}",
                "INFO",
                "PERMISOS",
                "PermissionRequest",
                request.Id);
        }

        return await GetRequestByIdAsync(request.Id) ?? throw new Exception("Error al crear solicitud");
    }

    public async Task<PermissionRequestDto> ApproveRequestAsync(int requestId, int approverUserId, string? comments = null)
    {
        var request = await _context.PermissionRequests
            .Include(pr => pr.Employee)
                .ThenInclude(e => e.User)
            .Include(pr => pr.PermissionType)
            .FirstOrDefaultAsync(pr => pr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        if (request.Status != RequestStatus.Pendiente && request.Status != RequestStatus.EnRevision)
            throw new InvalidOperationException("Solo se pueden aprobar solicitudes pendientes o en revisión");

        var oldStatus = request.Status;
        request.Status = RequestStatus.Aprobada;
        request.ApprovedByUserId = approverUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApproverComments = comments;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(approverUserId, "APPROVE", "PERMISOS", "PermissionRequest", requestId,
            oldValues: new { Status = oldStatus.ToString() },
            newValues: new { Status = RequestStatus.Aprobada.ToString() },
            description: $"Permiso aprobado: {request.PermissionType?.Name}");

        // Notificar al empleado
        if (request.Employee?.User != null)
        {
            await _notificationService.NotifyRequestStatusChangeAsync(
                request.Employee.User.Id, "Permiso", requestId, "Aprobada", comments);
        }

        return await GetRequestByIdAsync(requestId) ?? throw new Exception("Error al aprobar solicitud");
    }

    public async Task<PermissionRequestDto> RejectRequestAsync(int requestId, int approverUserId, string reason)
    {
        var request = await _context.PermissionRequests
            .Include(pr => pr.Employee)
                .ThenInclude(e => e.User)
            .Include(pr => pr.PermissionType)
            .FirstOrDefaultAsync(pr => pr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        if (request.Status != RequestStatus.Pendiente && request.Status != RequestStatus.EnRevision)
            throw new InvalidOperationException("Solo se pueden rechazar solicitudes pendientes o en revisión");

        var oldStatus = request.Status;
        request.Status = RequestStatus.Rechazada;
        request.ApprovedByUserId = approverUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApproverComments = reason;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(approverUserId, "REJECT", "PERMISOS", "PermissionRequest", requestId,
            oldValues: new { Status = oldStatus.ToString() },
            newValues: new { Status = RequestStatus.Rechazada.ToString(), Reason = reason },
            description: $"Permiso rechazado: {reason}");

        // Notificar al empleado
        if (request.Employee?.User != null)
        {
            await _notificationService.NotifyRequestStatusChangeAsync(
                request.Employee.User.Id, "Permiso", requestId, "Rechazada", reason);
        }

        return await GetRequestByIdAsync(requestId) ?? throw new Exception("Error al rechazar solicitud");
    }

    public async Task<PermissionRequestDto> CancelRequestAsync(int requestId, int userId, string? reason = null)
    {
        var request = await _context.PermissionRequests.FindAsync(requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        if (request.Status == RequestStatus.Cancelada)
            throw new InvalidOperationException("La solicitud ya está cancelada");

        if (request.Status == RequestStatus.Aprobada && request.StartDate <= DateTime.Today)
            throw new InvalidOperationException("No se puede cancelar una solicitud aprobada cuya fecha ya pasó");

        var oldStatus = request.Status;
        request.Status = RequestStatus.Cancelada;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(userId, "CANCEL", "PERMISOS", "PermissionRequest", requestId,
            oldValues: new { Status = oldStatus.ToString() },
            newValues: new { Status = RequestStatus.Cancelada.ToString() },
            description: reason ?? "Solicitud de permiso cancelada");

        return await GetRequestByIdAsync(requestId) ?? throw new Exception("Error al cancelar solicitud");
    }

    public async Task<PermissionUsageSummaryDto> GetUsageSummaryAsync(int employeeId, int year)
    {
        var permissionTypes = await _context.PermissionTypes
            .Where(pt => pt.IsActive)
            .ToListAsync();

        var usageByType = new Dictionary<string, decimal>();

        foreach (var type in permissionTypes)
        {
            var used = await _context.PermissionRequests
                .Where(pr => pr.EmployeeId == employeeId 
                    && pr.PermissionTypeId == type.Id
                    && pr.StartDate.Year == year
                    && pr.Status == RequestStatus.Aprobada)
                .SumAsync(pr => pr.DurationDays);

            usageByType[type.Name] = used;
        }

        return new PermissionUsageSummaryDto
        {
            EmployeeId = employeeId,
            Year = year,
            UsageByType = usageByType,
            TotalDaysUsed = usageByType.Values.Sum()
        };
    }

    // === MÉTODOS AUXILIARES ===

    private async Task<User?> GetSupervisorUserAsync(int employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Supervisor)
                .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        return employee?.Supervisor?.User;
    }

    private static PermissionRequestDto MapToDto(PermissionRequest pr)
    {
        return new PermissionRequestDto
        {
            Id = pr.Id,
            EmployeeId = pr.EmployeeId,
            EmployeeName = pr.Employee?.FullName ?? string.Empty,
            PermissionTypeId = pr.PermissionTypeId,
            PermissionTypeName = pr.PermissionType?.Name ?? string.Empty,
            StartDate = pr.StartDate,
            EndDate = pr.EndDate,
            StartTime = pr.StartTime,
            EndTime = pr.EndTime,
            IsPartialDay = pr.IsPartialDay,
            DurationDays = pr.DurationDays,
            Reason = pr.Reason,
            DocumentUrl = pr.DocumentUrl,
            Status = pr.Status.ToString(),
            ApprovedByUserId = pr.ApprovedByUserId,
            ApprovedByUserName = pr.ApprovedByUser?.Username,
            ApprovedAt = pr.ApprovedAt,
            ApproverComments = pr.ApproverComments,
            CreatedAt = pr.CreatedAt,
            UpdatedAt = pr.UpdatedAt
        };
    }
}
