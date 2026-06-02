using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Vacations;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class VacationService : IVacationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public VacationService(
        ApplicationDbContext context,
        INotificationService notificationService,
        IAuditService auditService)
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

    // Si no existe el saldo, crearlo automáticamente
    // Mínimo legal CR: 14 días hábiles por año (Art. 153 Código de Trabajo)
    if (balance == null)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return null;

        int totalDays = employee.VacationDaysPerYear > 0
            ? employee.VacationDaysPerYear
            : 14;

        // Trasladar días disponibles del año anterior si existen
        var prevBalance = await _context.VacationBalances
            .FirstOrDefaultAsync(v => v.EmployeeId == employeeId && v.Year == year - 1);

        decimal carriedOver = prevBalance != null
            ? Math.Max(0, prevBalance.AvailableDays)
            : 0;

        balance = new VacationBalance
{
    EmployeeId      = employeeId,
    Year            = year,
    TotalDays       = totalDays,
    CarriedOverDays = (int)Math.Floor(carriedOver),
    UsedDays        = 0,
    PendingDays     = 0,
    ExpirationDate  = new DateTime(year + 1, 12, 31),
    CreatedAt       = DateTime.UtcNow
};

        _context.VacationBalances.Add(balance);
        await _context.SaveChangesAsync();
    }
        return new VacationBalanceDto
        {
            Id = balance.Id,
            EmployeeId = balance.EmployeeId,
            EmployeeName = balance.Employee?.FullName,
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
        var balances = await _context.VacationBalances
            .Include(vb => vb.Employee)
            .Where(vb => vb.EmployeeId == employeeId)
            .OrderByDescending(vb => vb.Year)
            .ToListAsync();

        return balances.Select(vb => new VacationBalanceDto
        {
            Id = vb.Id,
            EmployeeId = vb.EmployeeId,
            EmployeeName = vb.Employee?.FullName,
            Year = vb.Year,
            TotalDays = vb.TotalDays,
            UsedDays = vb.UsedDays,
            PendingDays = vb.PendingDays,
            AvailableDays = vb.AvailableDays,
            CarriedOverDays = vb.CarriedOverDays,
            ExpirationDate = vb.ExpirationDate
        });
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

        int carriedOver = 0;

        var previousBalance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == employeeId && vb.Year == year - 1);

        if (previousBalance != null && previousBalance.AvailableDays > 0)
        {
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
            EmployeeName = employee.FullName,
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
            .Include(vr => vr.RequestStatus)
            .Include(vr => vr.ApprovedByUser)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

        if (request == null)
            return null;

        return MapToDto(request);
    }

    public async Task<IEnumerable<VacationRequestDto>> GetEmployeeRequestsAsync(int employeeId)
    {
        var requests = await _context.VacationRequests
            .Include(vr => vr.Employee)
            .Include(vr => vr.RequestStatus)
            .Include(vr => vr.ApprovedByUser)
            .Where(vr => vr.EmployeeId == employeeId)
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<IEnumerable<VacationRequestDto>> GetPendingRequestsAsync()
    {
        var pendingStatusIds = await GetExistingRequestStatusIdsAsync(
            "Pendiente",
            "En Revision",
            "En Revisión"
        );

        var requests = await _context.VacationRequests
            .Include(vr => vr.Employee)
            .Include(vr => vr.RequestStatus)
            .Include(vr => vr.ApprovedByUser)
            .Where(vr => pendingStatusIds.Contains(vr.RequestStatusId))
            .OrderBy(vr => vr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<IEnumerable<VacationRequestDto>> GetAllRequestsAsync(VacationRequestFilterDto? filter = null)
    {
        var query = _context.VacationRequests
            .Include(vr => vr.Employee)
            .Include(vr => vr.RequestStatus)
            .Include(vr => vr.ApprovedByUser)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
            {
                query = query.Where(vr => vr.EmployeeId == filter.EmployeeId.Value);
            }

            if (filter.RequestStatusId.HasValue)
            {
                query = query.Where(vr => vr.RequestStatusId == filter.RequestStatusId.Value);
            }

            if (filter.StartDateFrom.HasValue)
            {
                query = query.Where(vr => vr.StartDate >= filter.StartDateFrom.Value);
            }

            if (filter.StartDateTo.HasValue)
            {
                query = query.Where(vr => vr.StartDate <= filter.StartDateTo.Value);
            }

            if (filter.Year.HasValue)
            {
                query = query.Where(vr => vr.StartDate.Year == filter.Year.Value);
            }
        }

        var requests = await query
            .OrderByDescending(vr => vr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<VacationRequestDto> CreateRequestAsync(CreateVacationRequestDto dto, int userId)
    {
        var employee = await _context.Employees.FindAsync(dto.EmployeeId);

        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");

        if (dto.StartDate < DateTime.Today)
            throw new InvalidOperationException("La fecha de inicio no puede ser en el pasado");

        if (dto.EndDate < dto.StartDate)
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la fecha de inicio");

        var requestedDays = await CalculateVacationDaysAsync(dto.StartDate, dto.EndDate);

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
        {
            throw new InvalidOperationException(
                $"Saldo insuficiente. Disponible: {balance.AvailableDays} días, Solicitado: {requestedDays} días");
        }

        var pendingStatus = await GetRequestStatusAsync("Pendiente");
        var approvedStatus = await GetRequestStatusAsync("Aprobada");

        var overlapStatusIds = await GetExistingRequestStatusIdsAsync(
            "Pendiente",
            "Aprobada",
            "En Revision",
            "En Revisión"
        );

        var overlappingRequest = await _context.VacationRequests
            .Where(vr =>
                vr.EmployeeId == dto.EmployeeId &&
                overlapStatusIds.Contains(vr.RequestStatusId) &&
                vr.StartDate <= dto.EndDate &&
                vr.EndDate >= dto.StartDate)
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
            RequestStatusId = pendingStatus.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.VacationRequests.Add(request);

        balance.PendingDays += requestedDays;

        await _context.SaveChangesAsync();

        await AddHistoryAsync(request.Id, pendingStatus.Id, userId, "Solicitud creada");

        await _auditService.LogAsync(
            userId,
            "CREATE",
            "VACACIONES",
            "VacationRequest",
            request.Id,
            newValues: new { request.StartDate, request.EndDate, request.RequestedDays },
            description: $"Nueva solicitud de vacaciones: {request.RequestedDays} días"
        );

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

        return await GetRequestByIdAsync(request.Id)
            ?? throw new Exception("Error al crear solicitud");
    }

    public async Task<VacationRequestDto> UpdateRequestAsync(int requestId, UpdateVacationRequestDto dto, int userId)
    {
        var request = await _context.VacationRequests
            .Include(vr => vr.Employee)
            .Include(vr => vr.RequestStatus)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        var pendingStatus = await GetRequestStatusAsync("Pendiente");

        if (request.RequestStatusId != pendingStatus.Id)
            throw new InvalidOperationException("Solo se pueden modificar solicitudes pendientes");

        var oldValues = new
        {
            request.StartDate,
            request.EndDate,
            request.RequestedDays,
            request.Reason
        };

        var newRequestedDays = await CalculateVacationDaysAsync(dto.StartDate, dto.EndDate);
        var daysDifference = newRequestedDays - request.RequestedDays;

        if (daysDifference > 0)
        {
            var balance = await _context.VacationBalances
                .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == dto.StartDate.Year);

            if (balance == null || balance.AvailableDays < daysDifference)
                throw new InvalidOperationException("Saldo insuficiente para la modificación");
        }

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

        await AddHistoryAsync(requestId, request.RequestStatusId, userId, "Solicitud modificada");

        await _auditService.LogAsync(
            userId,
            "UPDATE",
            "VACACIONES",
            "VacationRequest",
            requestId,
            oldValues: oldValues,
            newValues: new { request.StartDate, request.EndDate, request.RequestedDays, request.Reason },
            description: "Solicitud de vacaciones modificada"
        );

        return await GetRequestByIdAsync(requestId)
            ?? throw new Exception("Error al actualizar solicitud");
    }

    public async Task<VacationRequestDto> ApproveRequestAsync(int requestId, int approverUserId, string? comments = null)
    {
        var request = await _context.VacationRequests
            .Include(vr => vr.Employee)
                .ThenInclude(e => e!.User)
            .Include(vr => vr.RequestStatus)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        var allowedStatusIds = await GetExistingRequestStatusIdsAsync(
            "Pendiente",
            "En Revision",
            "En Revisión"
        );

        if (!allowedStatusIds.Contains(request.RequestStatusId))
            throw new InvalidOperationException("Solo se pueden aprobar solicitudes pendientes o en revisión");

        var approvedStatus = await GetRequestStatusAsync("Aprobada");
        var oldStatusName = request.RequestStatus?.Name ?? string.Empty;

        request.RequestStatusId = approvedStatus.Id;
        request.ApprovedByUserId = approverUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApproverComments = comments;
        request.UpdatedAt = DateTime.UtcNow;

        var balance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == request.StartDate.Year);

        if (balance != null)
        {
            balance.PendingDays -= request.RequestedDays;
            balance.UsedDays += request.RequestedDays;
        }

        await _context.SaveChangesAsync();

        await AddHistoryAsync(requestId, approvedStatus.Id, approverUserId, comments ?? "Solicitud aprobada");

        await _auditService.LogAsync(
            approverUserId,
            "APPROVE",
            "VACACIONES",
            "VacationRequest",
            requestId,
            oldValues: new { Status = oldStatusName },
            newValues: new { Status = "Aprobada" },
            description: $"Vacaciones aprobadas: {request.RequestedDays} días"
        );

        if (request.Employee?.User != null)
        {
            await _notificationService.NotifyRequestStatusChangeAsync(
                request.Employee.User.Id,
                "Vacaciones",
                requestId,
                "Aprobada",
                comments);
        }

        return await GetRequestByIdAsync(requestId)
            ?? throw new Exception("Error al aprobar solicitud");
    }

    public async Task<VacationRequestDto> RejectRequestAsync(int requestId, int approverUserId, string reason)
    {
        var request = await _context.VacationRequests
            .Include(vr => vr.Employee)
                .ThenInclude(e => e!.User)
            .Include(vr => vr.RequestStatus)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        var allowedStatusIds = await GetExistingRequestStatusIdsAsync(
            "Pendiente",
            "En Revision",
            "En Revisión"
        );

        if (!allowedStatusIds.Contains(request.RequestStatusId))
            throw new InvalidOperationException("Solo se pueden rechazar solicitudes pendientes o en revisión");

        var rejectedStatus = await GetRequestStatusAsync("Rechazada");
        var oldStatusName = request.RequestStatus?.Name ?? string.Empty;

        request.RequestStatusId = rejectedStatus.Id;
        request.ApprovedByUserId = approverUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApproverComments = reason;
        request.UpdatedAt = DateTime.UtcNow;

        var balance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == request.StartDate.Year);

        if (balance != null)
            balance.PendingDays -= request.RequestedDays;

        await _context.SaveChangesAsync();

        await AddHistoryAsync(requestId, rejectedStatus.Id, approverUserId, reason);

        await _auditService.LogAsync(
            approverUserId,
            "REJECT",
            "VACACIONES",
            "VacationRequest",
            requestId,
            oldValues: new { Status = oldStatusName },
            newValues: new { Status = "Rechazada", Reason = reason },
            description: $"Vacaciones rechazadas: {reason}"
        );

        if (request.Employee?.User != null)
        {
            await _notificationService.NotifyRequestStatusChangeAsync(
                request.Employee.User.Id,
                "Vacaciones",
                requestId,
                "Rechazada",
                reason);
        }

        return await GetRequestByIdAsync(requestId)
            ?? throw new Exception("Error al rechazar solicitud");
    }

    public async Task<VacationRequestDto> CancelRequestAsync(int requestId, int userId, string? reason = null)
    {
        var request = await _context.VacationRequests
            .Include(vr => vr.RequestStatus)
            .FirstOrDefaultAsync(vr => vr.Id == requestId);

        if (request == null)
            throw new ArgumentException("Solicitud no encontrada");

        var cancelledStatus = await GetRequestStatusAsync("Cancelada");
        var approvedStatus = await GetRequestStatusAsync("Aprobada");

        if (request.RequestStatusId == cancelledStatus.Id)
            throw new InvalidOperationException("La solicitud ya está cancelada");

        if (request.RequestStatusId == approvedStatus.Id && request.StartDate <= DateTime.Today)
            throw new InvalidOperationException("No se puede cancelar una solicitud aprobada que ya inició");

        var oldStatusName = request.RequestStatus?.Name ?? string.Empty;

        var balance = await _context.VacationBalances
            .FirstOrDefaultAsync(vb => vb.EmployeeId == request.EmployeeId && vb.Year == request.StartDate.Year);

        if (balance != null)
        {
            if (request.RequestStatusId == approvedStatus.Id)
            {
                balance.UsedDays -= request.RequestedDays;
            }
            else
            {
                balance.PendingDays -= request.RequestedDays;
            }
        }

        request.RequestStatusId = cancelledStatus.Id;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await AddHistoryAsync(requestId, cancelledStatus.Id, userId, reason ?? "Solicitud cancelada");

        await _auditService.LogAsync(
            userId,
            "CANCEL",
            "VACACIONES",
            "VacationRequest",
            requestId,
            oldValues: new { Status = oldStatusName },
            newValues: new { Status = "Cancelada" },
            description: reason ?? "Solicitud de vacaciones cancelada"
        );

        return await GetRequestByIdAsync(requestId)
            ?? throw new Exception("Error al cancelar solicitud");
    }

    public async Task<IEnumerable<VacationRequestHistoryDto>> GetRequestHistoryAsync(int requestId)
    {
        var history = await _context.VacationRequestHistory
            .Include(h => h.ChangedByUser)
            .Include(h => h.RequestStatus)
            .Where(h => h.VacationRequestId == requestId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return history.Select(h => new VacationRequestHistoryDto
        {
            Id = h.Id,
            RequestStatusId = h.RequestStatusId,
            RequestStatusName = h.RequestStatus?.Name ?? string.Empty,
            Comments = h.Comments,
            ChangedByUserName = h.ChangedByUser != null ? h.ChangedByUser.Username : "Sistema",
            CreatedAt = h.CreatedAt
        });
    }

    // === MÉTODOS AUXILIARES ===

    /// <summary>
    /// Calcula días hábiles excluyendo sábados, domingos Y feriados nacionales.
    /// Art. 152 Código de Trabajo CR: los feriados no se descuentan de vacaciones.
    /// </summary>
    private async Task<int> CalculateVacationDaysAsync(DateTime start, DateTime end)
    {
        // Obtener feriados del período (columna HolidayDate)
        var holidays = await _context.PublicHolidays
            .Where(h => h.IsActive && h.Date >= start.Date && h.Date <= end.Date)
            .Select(h => h.Date.Date)
            .ToListAsync();

        int days = 0;
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            // Excluir fines de semana y feriados
            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday &&
                !holidays.Contains(date))
            {
                days++;
            }
        }
        return days;
    }

    // Versión sincrónica para compatibilidad (sin consulta a BD)
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

    private async Task AddHistoryAsync(int requestId, int requestStatusId, int userId, string? comments)
    {
        var history = new VacationRequestHistory
        {
            VacationRequestId = requestId,
            RequestStatusId = requestStatusId,
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

    private async Task<RequestStatus> GetRequestStatusAsync(string name)
    {
        var status = await _context.RequestStatuses
            .FirstOrDefaultAsync(rs => rs.Name == name);

        if (status == null)
            throw new InvalidOperationException($"No existe el estado de solicitud: {name}");

        return status;
    }

    private async Task<List<int>> GetExistingRequestStatusIdsAsync(params string[] names)
    {
        var ids = await _context.RequestStatuses
            .Where(rs => names.Contains(rs.Name))
            .Select(rs => rs.Id)
            .ToListAsync();

        if (!ids.Any())
            throw new InvalidOperationException("No existen estados de solicitud configurados");

        return ids;
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
            RequestStatusId = vr.RequestStatusId,
            RequestStatusName = vr.RequestStatus?.Name ?? string.Empty,
            ApprovedByUserId = vr.ApprovedByUserId,
            ApprovedByUserName = vr.ApprovedByUser?.Username,
            ApprovedAt = vr.ApprovedAt,
            ApproverComments = vr.ApproverComments,
            CreatedAt = vr.CreatedAt,
            UpdatedAt = vr.UpdatedAt
        };
    }
}