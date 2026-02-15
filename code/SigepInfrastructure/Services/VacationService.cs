using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Vacations;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepDomain.Enums;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class VacationService : IVacationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public VacationService(ApplicationDbContext context, INotificationService notificationService, IAuditService auditService)
    {
        _context = context;
        _notificationService = notificationService;
        _auditService = auditService;
    }

    // === SALDO DE VACACIONES ===

    public async Task<VacationBalanceDto?> GetBalanceAsync(int employeeId, int year)
    {
        var balance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == employeeId && vb.Year == year);

        if (balance == null)
            return null;

        return new VacationBalanceDto
        {
            Id = balance.Id,
            EmployeeId = balance.EmployeeId,
            Year = balance.Year,
            TotalDays = balance.TotalDays,
            UsedDays = balance.UsedDays,
            PendingDays = balance.PendingDays,
            AvailableDays = balance.AvailableDays,
            CarriedOverDays = balance.CarriedOverDays,
            ExpirationDate = balance.ExpirationDate
        };
    }

    public async Task<IEnumerable<VacationBalanceDto>> GetBalanceHistoryAsync(int employeeId)
    {
        return await _context.VacationBalances
            .Where(vb => vb.EmployeeId == employeeId)
            .OrderByDescending(vb => vb.Year)
            .Select(vb => new VacationBalanceDto
            {
                Id = vb.Id,
                EmployeeId = vb.EmployeeId,
                Year = vb.Year,
                TotalDays = vb.TotalDays,
                UsedDays = vb.UsedDays,
                PendingDays = vb.PendingDays,
                AvailableDays = vb.AvailableDays,
                CarriedOverDays = vb.CarriedOverDays,
                ExpirationDate = vb.ExpirationDate
            })
            .ToListAsync();
    }

    public async Task<VacationBalanceDto> InitializeBalanceAsync(int employeeId, int year)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");

        var existingBalance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == employeeId && vb.Year == year);

        if (existingBalance != null)
            throw new InvalidOperationException($"Ya existe un saldo para el año {year}");

        // Obtener días acarreados del año anterior
        decimal carriedOver = 0;
        var previousBalance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == employeeId && vb.Year == year - 1);
        
        if (previousBalance != null && previousBalance.AvailableDays > 0)
        {
            // Máximo 5 días de acarreo (configurable)
            carriedOver = Math.Min(previousBalance.AvailableDays, 5);
        }

        var balance = new VacationBalance
        {
            EmployeeId = employeeId,
            Year = year,
            TotalDays = employee.VacationDaysPerYear + carriedOver,
            UsedDays = 0,
            PendingDays = 0,
            CarriedOverDays = carriedOver,
            ExpirationDate = new DateTime(year, 12, 31)
        };

        _context.VacationBalances.Add(balance);
        await _context.SaveChangesAsync();

        return new VacationBalanceDto
        {
            Id = balance.Id,
            EmployeeId = balance.EmployeeId,
            Year = balance.Year,
            TotalDays = balance.TotalDays,
            UsedDays = balance.UsedDays,
            PendingDays = balance.PendingDays,
            AvailableDays = balance.AvailableDays,
            CarriedOverDays = balance.CarriedOverDays,
            ExpirationDate = balance.ExpirationDate
        };
    }

    // === SOLICITUDES DE VACACIONES ===

    public async Task<VacationRequestDto?> GetRequestByIdAsync(int requestId)
    {
        var request = await _context.VacationRequests
            .Include(vr => vr.Employee)
            .Include(vr => vr.ApprovedByUser)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

        if (request == null)
            return null;

        return MapToDto(request);
    }

    public async Task<IEnumerable<VacationRequestDto>> GetEmployeeRequestsAsync(int employeeId)
    {
        return await _context.VacationRequests
            .Include(vr => vr.Employee)
            .Include(vr => vr.ApprovedByUser)
            .Where(vr => vr.EmployeeId == employeeId)
            .OrderByDescending(vr => vr.CreatedAt)
            .Select(vr => MapToDto(vr))
            .ToListAsync();
    }

    public async Task<IEnumerable<VacationRequestDto>> GetPendingRequestsAsync()
    {
        return await _context.VacationRequests
            .Include(vr => vr.Employee)
            .Where(vr => vr.Status == RequestStatus.Pendiente || vr.Status == RequestStatus.EnRevision)
            .OrderBy(vr => vr.CreatedAt)
            .Select(vr => MapToDto(vr))
            .ToListAsync();
    }

    public async Task<IEnumerable<VacationRequestDto>> GetAllRequestsAsync(VacationRequestFilterDto? filter = null)
    {
        var query = _context.VacationRequests
            .Include(vr => vr.Employee)
            .Include(vr => vr.ApprovedByUser)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
                query = query.Where(vr => vr.EmployeeId == filter.EmployeeId.Value);
            
            if (filter.Status.HasValue)
                query = query.Where(vr => vr.Status == filter.Status.Value);
            
            if (filter.StartDateFrom.HasValue)
                query = query.Where(vr => vr.StartDate >= filter.StartDateFrom.Value);
            
            if (filter.StartDateTo.HasValue)
                query = query.Where(vr => vr.StartDate <= filter.StartDateTo.Value);
            
            if (filter.Year.HasValue)
                query = query.Where(vr => vr.StartDate.Year == filter.Year.Value);
        }

        return await query
            .OrderByDescending(vr => vr.CreatedAt)
            .Select(vr => MapToDto(vr))
            .ToListAsync();
    }

    public async Task<VacationRequestDto> CreateRequestAsync(CreateVacationRequestDto dto, int userId)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");

        // Validar fechas
        if (dto.StartDate < DateTime.Today)
            throw new InvalidOperationException("La fecha de inicio no puede ser en el pasado");
        
        if (dto.EndDate < dto.StartDate)
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la fecha de inicio");

        // Calcular días hábiles
        var requestedDays = CalculateBusinessDays(dto.StartDate, dto.EndDate);
        
        // Verificar saldo disponible
        var year = dto.StartDate.Year;
        var balance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == dto.EmployeeId && vb.Year == year);
        
        if (balance == null)
        {
            balance = new VacationBalance
            {
                EmployeeId = dto.EmployeeId,
                Year = year,
                TotalDays = employee.VacationDaysPerYear,
                UsedDays = 0,
                PendingDays = 0,
                CarriedOverDays = 0,
                ExpirationDate = new DateTime(year, 12, 31)
            };
            _context.VacationBalances.Add(balance);
            await _context.SaveChangesAsync();
        }

        if (balance.AvailableDays < requestedDays)
            throw new InvalidOperationException($"Saldo insuficiente. Disponible: {balance.AvailableDays} días, Solicitado: {requestedDays} días");

        // Verificar solapamiento con otras solicitudes aprobadas o pendientes
        var overlappingRequest = await _context.VacationRequests
            .Where(vr => vr.EmployeeId == dto.EmployeeId 
                && (vr.Status == RequestStatus.Pendiente || vr.Status == RequestStatus.Aprobada || vr.Status == RequestStatus.EnRevision)
                && vr.StartDate <= dto.EndDate 
                && vr.EndDate >= dto.StartDate)
            .FirstOrDefaultAsync();

        if (overlappingRequest != null)
            throw new InvalidOperationException("Ya existe una solicitud que se solapa con las fechas seleccionadas");

        var request = new VacationRequest
        {
            EmployeeId = dto.EmployeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            RequestedDays = requestedDays,
            Reason = dto.Reason,
            Status = RequestStatus.Pendiente,
            CreatedAt = DateTime.UtcNow
        };

        _context.VacationRequests.Add(request);

        // Actualizar días pendientes
        balance.PendingDays += requestedDays;

        await _context.SaveChangesAsync();

        // Registrar historial
        await AddHistoryAsync(request.Id, RequestStatus.Pendiente, userId, "Solicitud creada");

        // Auditoría
        await _auditService.LogAsync(userId, "CREATE", "VACACIONES", "VacationRequest", request.Id, 
            newValues: new { request.StartDate, request.EndDate, request.RequestedDays },
            description: $"Nueva solicitud de vacaciones: {request.RequestedDays} días");

        // Notificar a supervisores/RRHH
        var supervisorUser = await GetSupervisorUserAsync(employee.Id);
        if (supervisorUser != null)
        {
            await _notificationService.CreateNotificationAsync(
                supervisorUser.Id,
                "Nueva solicitud de vacaciones",
                $"{employee.FullName} ha solicitado {requestedDays} días de vacaciones del {dto.StartDate:dd/MM/yyyy} al {dto.EndDate:dd/MM/yyyy}",
                "INFO",
                "VACACIONES",
                "VacationRequest",
                request.Id);
        }

        return await GetRequestByIdAsync(request.Id) ?? throw new Exception("Error al crear solicitud");
    }

    public async Task<VacationRequestDto> UpdateRequestAsync(int requestId, UpdateVacationRequestDto dto, int userId)
    {
        var request = await _context.VacationRequests
            .Include(vr => vr.Employee)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        if (request.Status != RequestStatus.Pendiente)
            throw new InvalidOperationException("Solo se pueden modificar solicitudes pendientes");

        var oldValues = new { request.StartDate, request.EndDate, request.RequestedDays, request.Reason };

        // Recalcular días
        var newRequestedDays = CalculateBusinessDays(dto.StartDate, dto.EndDate);
        var daysDifference = newRequestedDays - request.RequestedDays;

        // Verificar saldo si aumentaron los días
        if (daysDifference > 0)
        {
            var balance = await _context.VacationBalances
                .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == dto.StartDate.Year);
            
            if (balance == null || (balance.AvailableDays - balance.PendingDays) < daysDifference)
                throw new InvalidOperationException("Saldo insuficiente para la modificación");
        }

        // Actualizar balance pendiente
        var oldBalance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == request.StartDate.Year);
        
        if (oldBalance != null)
            oldBalance.PendingDays -= request.RequestedDays;

        var newBalance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == dto.StartDate.Year);
        
        if (newBalance != null)
            newBalance.PendingDays += newRequestedDays;

        request.StartDate = dto.StartDate;
        request.EndDate = dto.EndDate;
        request.RequestedDays = newRequestedDays;
        request.Reason = dto.Reason;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await AddHistoryAsync(requestId, request.Status, userId, "Solicitud modificada");

        await _auditService.LogAsync(userId, "UPDATE", "VACACIONES", "VacationRequest", requestId,
            oldValues: oldValues,
            newValues: new { request.StartDate, request.EndDate, request.RequestedDays, request.Reason },
            description: "Solicitud de vacaciones modificada");

        return await GetRequestByIdAsync(requestId) ?? throw new Exception("Error al actualizar solicitud");
    }

    public async Task<VacationRequestDto> ApproveRequestAsync(int requestId, int approverUserId, string? comments = null)
    {
        var request = await _context.VacationRequests
            .Include(vr => vr.Employee)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

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

        // Actualizar balance: quitar de pendientes, agregar a usados
        var balance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == request.StartDate.Year);
        
        if (balance != null)
        {
            balance.PendingDays -= request.RequestedDays;
            balance.UsedDays += request.RequestedDays;
        }

        await _context.SaveChangesAsync();

        await AddHistoryAsync(requestId, RequestStatus.Aprobada, approverUserId, comments ?? "Solicitud aprobada");

        await _auditService.LogAsync(approverUserId, "APPROVE", "VACACIONES", "VacationRequest", requestId,
            oldValues: new { Status = oldStatus.ToString() },
            newValues: new { Status = RequestStatus.Aprobada.ToString() },
            description: $"Vacaciones aprobadas: {request.RequestedDays} días");

        // Notificar al empleado
        if (request.Employee?.User != null)
        {
            await _notificationService.NotifyRequestStatusChangeAsync(
                request.Employee.User.Id, "Vacaciones", requestId, "Aprobada", comments);
        }

        return await GetRequestByIdAsync(requestId) ?? throw new Exception("Error al aprobar solicitud");
    }

    public async Task<VacationRequestDto> RejectRequestAsync(int requestId, int approverUserId, string reason)
    {
        var request = await _context.VacationRequests
            .Include(vr => vr.Employee)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

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

        // Devolver días pendientes al saldo
        var balance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == request.StartDate.Year);
        
        if (balance != null)
            balance.PendingDays -= request.RequestedDays;

        await _context.SaveChangesAsync();

        await AddHistoryAsync(requestId, RequestStatus.Rechazada, approverUserId, reason);

        await _auditService.LogAsync(approverUserId, "REJECT", "VACACIONES", "VacationRequest", requestId,
            oldValues: new { Status = oldStatus.ToString() },
            newValues: new { Status = RequestStatus.Rechazada.ToString(), Reason = reason },
            description: $"Vacaciones rechazadas: {reason}");

        // Notificar al empleado
        if (request.Employee?.User != null)
        {
            await _notificationService.NotifyRequestStatusChangeAsync(
                request.Employee.User.Id, "Vacaciones", requestId, "Rechazada", reason);
        }

        return await GetRequestByIdAsync(requestId) ?? throw new Exception("Error al rechazar solicitud");
    }

    public async Task<VacationRequestDto> CancelRequestAsync(int requestId, int userId, string? reason = null)
    {
        var request = await _context.VacationRequests.FindAsync(requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        if (request.Status == RequestStatus.Cancelada)
            throw new InvalidOperationException("La solicitud ya está cancelada");

        if (request.Status == RequestStatus.Aprobada && request.StartDate <= DateTime.Today)
            throw new InvalidOperationException("No se puede cancelar una solicitud aprobada que ya inició");

        var oldStatus = request.Status;
        
        // Restaurar balance
        var balance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == request.StartDate.Year);
        
        if (balance != null)
        {
            if (oldStatus == RequestStatus.Aprobada)
                balance.UsedDays -= request.RequestedDays;
            else if (oldStatus == RequestStatus.Pendiente || oldStatus == RequestStatus.EnRevision)
                balance.PendingDays -= request.RequestedDays;
        }

        request.Status = RequestStatus.Cancelada;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await AddHistoryAsync(requestId, RequestStatus.Cancelada, userId, reason ?? "Solicitud cancelada");

        await _auditService.LogAsync(userId, "CANCEL", "VACACIONES", "VacationRequest", requestId,
            oldValues: new { Status = oldStatus.ToString() },
            newValues: new { Status = RequestStatus.Cancelada.ToString() },
            description: reason ?? "Solicitud de vacaciones cancelada");

        return await GetRequestByIdAsync(requestId) ?? throw new Exception("Error al cancelar solicitud");
    }

    public async Task<IEnumerable<VacationRequestHistoryDto>> GetRequestHistoryAsync(int requestId)
    {
        return await _context.VacationRequestHistory
            .Include(h => h.ChangedByUser)
            .Where(h => h.VacationRequestId == requestId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new VacationRequestHistoryDto
            {
                Id = h.Id,
                Status = h.Status.ToString(),
                Comments = h.Comments,
                ChangedByUserName = h.ChangedByUser != null ? h.ChangedByUser.Username : "Sistema",
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();
    }

    // === MÉTODOS AUXILIARES ===

    private static int CalculateBusinessDays(DateTime start, DateTime end)
    {
        int days = 0;
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                days++;
        }
        return days;
    }

    private async Task AddHistoryAsync(int requestId, RequestStatus status, int userId, string? comments)
    {
        var history = new VacationRequestHistory
        {
            VacationRequestId = requestId,
            Status = status,
            Comments = comments,
            ChangedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.VacationRequestHistory.Add(history);
        await _context.SaveChangesAsync();
    }

    private async Task<User?> GetSupervisorUserAsync(int employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.Supervisor)
                .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        return employee?.Supervisor?.User;
    }

    private static VacationRequestDto MapToDto(VacationRequest vr)
    {
        return new VacationRequestDto
        {
            Id = vr.Id,
            EmployeeId = vr.EmployeeId,
            EmployeeName = vr.Employee?.FullName ?? string.Empty,
            StartDate = vr.StartDate,
            EndDate = vr.EndDate,
            RequestedDays = vr.RequestedDays,
            Reason = vr.Reason,
            Status = vr.Status.ToString(),
            ApprovedByUserId = vr.ApprovedByUserId,
            ApprovedByUserName = vr.ApprovedByUser?.Username,
            ApprovedAt = vr.ApprovedAt,
            ApproverComments = vr.ApproverComments,
            CreatedAt = vr.CreatedAt,
            UpdatedAt = vr.UpdatedAt
        };
    }
}
