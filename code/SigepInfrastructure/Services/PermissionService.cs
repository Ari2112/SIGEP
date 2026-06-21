using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Permissions;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public PermissionService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IAuditService auditService)
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

        if (pt == null)
            return null;

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
            .Include(pr => pr.RequestStatus)
            .Include(pr => pr.ApprovedByUser)
            .FirstOrDefaultAsync(pr => pr.Id == requestId);

        if (request == null)
            return null;

        return MapToDto(request);
    }

    public async Task<IEnumerable<PermissionRequestDto>> GetEmployeeRequestsAsync(int employeeId)
    {
        var requests = await _context.PermissionRequests
            .Include(pr => pr.Employee)
            .Include(pr => pr.PermissionType)
            .Include(pr => pr.RequestStatus)
            .Include(pr => pr.ApprovedByUser)
            .Where(pr => pr.EmployeeId == employeeId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<IEnumerable<PermissionRequestDto>> GetPendingRequestsAsync()
    {
        var pendingStatus = await GetRequestStatusAsync("Pendiente");

        var requests = await _context.PermissionRequests
            .Include(pr => pr.Employee)
            .Include(pr => pr.PermissionType)
            .Include(pr => pr.RequestStatus)
            .Include(pr => pr.ApprovedByUser)
            .Where(pr => pr.RequestStatusId == pendingStatus.Id)
            .OrderBy(pr => pr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<IEnumerable<PermissionRequestDto>> GetAllRequestsAsync(PermissionRequestFilterDto? filter = null)
    {
        var query = _context.PermissionRequests
            .Include(pr => pr.Employee)
            .Include(pr => pr.PermissionType)
            .Include(pr => pr.RequestStatus)
            .Include(pr => pr.ApprovedByUser)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
            {
                query = query.Where(pr => pr.EmployeeId == filter.EmployeeId.Value);
            }

            if (filter.PermissionTypeId.HasValue)
            {
                query = query.Where(pr => pr.PermissionTypeId == filter.PermissionTypeId.Value);
            }

            if (filter.RequestStatusId.HasValue)
            {
                query = query.Where(pr => pr.RequestStatusId == filter.RequestStatusId.Value);
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(pr => pr.StartDate >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(pr => pr.StartDate <= filter.DateTo.Value);
            }
        }

        var requests = await query
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
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

        if (dto.StartDate < DateTime.Today)
            throw new InvalidOperationException("La fecha de inicio no puede ser en el pasado");

        decimal durationDays;
        TimeSpan? parsedStartTime = null;
        TimeSpan? parsedEndTime = null;

        if (dto.IsPartialDay)
        {
            if (string.IsNullOrWhiteSpace(dto.StartTime) || string.IsNullOrWhiteSpace(dto.EndTime))
                throw new InvalidOperationException("Para permisos parciales debe especificar hora de inicio y fin");

            // El input HTML <input type="time"> envía "HH:mm" (ej. "09:00") sin segundos.
            // TimeSpan.TryParse acepta ese formato, a diferencia del deserializador JSON de TimeSpan.
            if (!TimeSpan.TryParse(dto.StartTime, out var startTime) ||
                !TimeSpan.TryParse(dto.EndTime, out var endTime))
                throw new InvalidOperationException("El formato de la hora no es válido");

            if (endTime <= startTime)
                throw new InvalidOperationException("La hora de fin debe ser posterior a la hora de inicio");

            parsedStartTime = startTime;
            parsedEndTime = endTime;

            var duration = endTime - startTime;
            durationDays = Math.Round((decimal)duration.TotalHours / 8m, 2);
        }
        else
        {
            durationDays = (decimal)(dto.EndDate ?? dto.StartDate)
                .Subtract(dto.StartDate)
                .TotalDays + 1;
        }

        var pendingStatus = await GetRequestStatusAsync("Pendiente");
        var approvedStatus = await GetRequestStatusAsync("Aprobada");

        if (permissionType.MaxDaysPerYear.HasValue)
        {
            var usedThisYear = await _context.PermissionRequests
                .Where(pr =>
                    pr.EmployeeId == dto.EmployeeId &&
                    pr.PermissionTypeId == dto.PermissionTypeId &&
                    pr.StartDate.Year == dto.StartDate.Year &&
                    (pr.RequestStatusId == approvedStatus.Id || pr.RequestStatusId == pendingStatus.Id))
                .SumAsync(pr => pr.DurationDays);

            if (usedThisYear + durationDays > permissionType.MaxDaysPerYear.Value)
            {
                throw new InvalidOperationException(
                    $"Ha excedido el límite anual de {permissionType.MaxDaysPerYear} días para este tipo de permiso. " +
                    $"Usado: {usedThisYear}, Solicitado: {durationDays}");
            }
        }

        var overlappingRequest = await _context.PermissionRequests
            .Where(pr =>
                pr.EmployeeId == dto.EmployeeId &&
                (pr.RequestStatusId == pendingStatus.Id || pr.RequestStatusId == approvedStatus.Id) &&
                pr.StartDate == dto.StartDate &&
                pr.IsPartialDay == dto.IsPartialDay)
            .FirstOrDefaultAsync();

        if (overlappingRequest != null && !dto.IsPartialDay)
            throw new InvalidOperationException("Ya existe una solicitud de permiso para la misma fecha");

        // Solo cita médica requiere comprobante obligatorio
        bool isMedical = permissionType.Name.Contains("dica", StringComparison.OrdinalIgnoreCase) ||
                         permissionType.Name.Contains("Cita", StringComparison.OrdinalIgnoreCase);

        if (isMedical && string.IsNullOrEmpty(dto.DocumentUrl))
            throw new InvalidOperationException("La cita médica requiere adjuntar el comprobante médico.");

        var request = new PermissionRequest
        {
            EmployeeId = dto.EmployeeId,
            PermissionTypeId = dto.PermissionTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate ?? dto.StartDate,
            StartTime = parsedStartTime,
            EndTime = parsedEndTime,
            IsPartialDay = dto.IsPartialDay,
            DurationDays = durationDays,
            Reason = dto.Reason,
            DocumentUrl = dto.DocumentUrl,
            RequestStatusId = pendingStatus.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.PermissionRequests.Add(request);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "CREATE",
            "PERMISOS",
            "PermissionRequest",
            request.Id,
            newValues: new { request.PermissionTypeId, request.StartDate, request.DurationDays },
            description: $"Nueva solicitud de permiso: {permissionType.Name}"
        );

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

        return await GetRequestByIdAsync(request.Id)
            ?? throw new Exception("Error al crear solicitud");
    }

    public async Task<PermissionRequestDto> ApproveRequestAsync(int requestId, int approverUserId, string? comments = null)
    {
        var request = await _context.PermissionRequests
            .Include(pr => pr.Employee)
                .ThenInclude(e => e!.User)
            .Include(pr => pr.PermissionType)
            .Include(pr => pr.RequestStatus)
            .FirstOrDefaultAsync(pr => pr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        var pendingStatus = await GetRequestStatusAsync("Pendiente");
        var approvedStatus = await GetRequestStatusAsync("Aprobada");

        if (request.RequestStatusId != pendingStatus.Id)
            throw new InvalidOperationException("Solo se pueden aprobar solicitudes pendientes");

        var oldStatusName = request.RequestStatus?.Name ?? string.Empty;

        request.RequestStatusId = approvedStatus.Id;
        request.ApprovedByUserId = approverUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApproverComments = comments;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            approverUserId,
            "APPROVE",
            "PERMISOS",
            "PermissionRequest",
            requestId,
            oldValues: new { Status = oldStatusName },
            newValues: new { Status = "Aprobada" },
            description: $"Permiso aprobado: {request.PermissionType?.Name}"
        );

        if (request.Employee?.User != null)
        {
            await _notificationService.NotifyRequestStatusChangeAsync(
                request.Employee.User.Id,
                "Permiso",
                requestId,
                "Aprobada",
                comments);
        }

        return await GetRequestByIdAsync(requestId)
            ?? throw new Exception("Error al aprobar solicitud");
    }

    public async Task<PermissionRequestDto> RejectRequestAsync(int requestId, int approverUserId, string reason)
    {
        var request = await _context.PermissionRequests
            .Include(pr => pr.Employee)
                .ThenInclude(e => e!.User)
            .Include(pr => pr.PermissionType)
            .Include(pr => pr.RequestStatus)
            .FirstOrDefaultAsync(pr => pr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        var pendingStatus = await GetRequestStatusAsync("Pendiente");
        var rejectedStatus = await GetRequestStatusAsync("Rechazada");

        if (request.RequestStatusId != pendingStatus.Id)
            throw new InvalidOperationException("Solo se pueden rechazar solicitudes pendientes");

        var oldStatusName = request.RequestStatus?.Name ?? string.Empty;

        request.RequestStatusId = rejectedStatus.Id;
        request.ApprovedByUserId = approverUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApproverComments = reason;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            approverUserId,
            "REJECT",
            "PERMISOS",
            "PermissionRequest",
            requestId,
            oldValues: new { Status = oldStatusName },
            newValues: new { Status = "Rechazada", Reason = reason },
            description: $"Permiso rechazado: {reason}"
        );

        if (request.Employee?.User != null)
        {
            await _notificationService.NotifyRequestStatusChangeAsync(
                request.Employee.User.Id,
                "Permiso",
                requestId,
                "Rechazada",
                reason);
        }

        return await GetRequestByIdAsync(requestId)
            ?? throw new Exception("Error al rechazar solicitud");
    }

    public async Task<PermissionRequestDto> CancelRequestAsync(int requestId, int userId, string? reason = null)
    {
        var request = await _context.PermissionRequests
            .Include(pr => pr.RequestStatus)
            .FirstOrDefaultAsync(pr => pr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        var approvedStatus = await GetRequestStatusAsync("Aprobada");
        var cancelledStatus = await GetRequestStatusAsync("Cancelada");

        if (request.RequestStatusId == cancelledStatus.Id)
            throw new InvalidOperationException("La solicitud ya está cancelada");

        if (request.RequestStatusId == approvedStatus.Id && request.StartDate <= DateTime.Today)
            throw new InvalidOperationException("No se puede cancelar una solicitud aprobada cuya fecha ya pasó");

        var oldStatusName = request.RequestStatus?.Name ?? string.Empty;

        request.RequestStatusId = cancelledStatus.Id;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "CANCEL",
            "PERMISOS",
            "PermissionRequest",
            requestId,
            oldValues: new { Status = oldStatusName },
            newValues: new { Status = "Cancelada" },
            description: reason ?? "Solicitud de permiso cancelada"
        );

        return await GetRequestByIdAsync(requestId)
            ?? throw new Exception("Error al cancelar solicitud");
    }

    public async Task<PermissionUsageSummaryDto> GetUsageSummaryAsync(int employeeId, int year)
    {
        var approvedStatus = await GetRequestStatusAsync("Aprobada");

        var permissionTypes = await _context.PermissionTypes
            .Where(pt => pt.IsActive)
            .ToListAsync();

        var usageByType = new Dictionary<string, decimal>();

        foreach (var type in permissionTypes)
        {
            var used = await _context.PermissionRequests
                .Where(pr =>
                    pr.EmployeeId == employeeId &&
                    pr.PermissionTypeId == type.Id &&
                    pr.StartDate.Year == year &&
                    pr.RequestStatusId == approvedStatus.Id)
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

    private async Task<RequestStatus> GetRequestStatusAsync(string name)
    {
        var status = await _context.RequestStatuses
            .FirstOrDefaultAsync(rs => rs.Name == name);

        if (status == null)
            throw new InvalidOperationException($"No existe el estado de solicitud: {name}");

        return status;
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
            RequestStatusId = pr.RequestStatusId,
            RequestStatusName = pr.RequestStatus?.Name ?? string.Empty,
            ApprovedByUserId = pr.ApprovedByUserId,
            ApprovedByUserName = pr.ApprovedByUser?.Username,
            ApprovedAt = pr.ApprovedAt,
            ApproverComments = pr.ApproverComments,
            CreatedAt = pr.CreatedAt,
            UpdatedAt = pr.UpdatedAt
        };
    }
}