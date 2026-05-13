using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Disability;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class DisabilityService : IDisabilityService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public DisabilityService(
        ApplicationDbContext context,
        IAuditService auditService,
        INotificationService notificationService)
    {
        _context = context;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<DisabilityRequestDto>> GetAllAsync(DisabilityFilterDto? filter = null)
    {
        var query = _context.DisabilityRequests
            .Include(dr => dr.Employee)
                .ThenInclude(e => e!.Position)
            .Include(dr => dr.DisabilityType)
            .Include(dr => dr.RequestStatus)
            .Include(dr => dr.ReviewedBy)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
            {
                query = query.Where(dr => dr.EmployeeId == filter.EmployeeId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(dr =>
                    dr.RequestStatus != null &&
                    dr.RequestStatus.Name == filter.Status);
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(dr => dr.StartDate >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(dr => dr.StartDate <= filter.DateTo.Value);
            }
        }

        var requests = await query
            .OrderByDescending(dr => dr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<IEnumerable<DisabilityRequestDto>> GetByEmployeeAsync(int employeeId)
    {
        var requests = await _context.DisabilityRequests
            .Include(dr => dr.Employee)
                .ThenInclude(e => e!.Position)
            .Include(dr => dr.DisabilityType)
            .Include(dr => dr.RequestStatus)
            .Include(dr => dr.ReviewedBy)
            .Where(dr => dr.EmployeeId == employeeId)
            .OrderByDescending(dr => dr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<DisabilityRequestDto?> GetByIdAsync(int id)
    {
        var dr = await _context.DisabilityRequests
            .Include(d => d.Employee)
                .ThenInclude(e => e!.Position)
            .Include(d => d.DisabilityType)
            .Include(d => d.RequestStatus)
            .Include(d => d.ReviewedBy)
            .FirstOrDefaultAsync(d => d.Id == id);

        return dr == null ? null : MapToDto(dr);
    }

    public async Task<DisabilityRequestDto> CreateAsync(int employeeId, CreateDisabilityDto dto)
    {
        var employee = await _context.Employees.FindAsync(employeeId)
            ?? throw new ArgumentException("Empleado no encontrado");

        if (dto.EndDate < dto.StartDate)
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio");

        int totalDays = (int)(dto.EndDate - dto.StartDate).TotalDays + 1;

        var pendingStatus = await _context.RequestStatuses
            .FirstAsync(rs => rs.Name == "Pendiente");

        var disabilityTypeExists = await _context.DisabilityTypes
            .AnyAsync(dt => dt.Id == dto.Type);

        if (!disabilityTypeExists)
            throw new ArgumentException("Tipo de incapacidad no válido");

        var request = new DisabilityRequest
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = totalDays,
            DisabilityTypeId = dto.Type,
            Diagnosis = dto.Diagnosis,
            DoctorName = dto.DoctorName,
            MedicalCenter = dto.MedicalCenter,
            DocumentNumber = dto.DocumentNumber,
            AttachmentPath = dto.AttachmentPath,
            RequestStatusId = pendingStatus.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.DisabilityRequests.Add(request);
        await _context.SaveChangesAsync();

        var hrUsers = await _context.Users
            .Include(u => u.Role)
            .Where(u =>
                u.Role != null &&
                (u.Role.Name == "RRHH" || u.Role.Name == "Admin"))
            .ToListAsync();

        foreach (var hrUser in hrUsers)
        {
            await _notificationService.CreateNotificationAsync(
                hrUser.Id,
                "Nueva incapacidad registrada",
                $"{employee.FullName} registró una incapacidad del {dto.StartDate:dd/MM/yyyy} al {dto.EndDate:dd/MM/yyyy} ({totalDays} días).",
                "WARNING",
                "INCAPACIDADES",
                "DisabilityRequest",
                request.Id);
        }

        return (await GetByIdAsync(request.Id))!;
    }

    public async Task<DisabilityRequestDto> ReviewAsync(
        int id,
        int reviewerUserId,
        bool approve,
        string? comments = null)
    {
        var request = await _context.DisabilityRequests
            .Include(dr => dr.Employee)
                .ThenInclude(e => e!.User)
            .Include(dr => dr.RequestStatus)
            .FirstOrDefaultAsync(dr => dr.Id == id)
            ?? throw new ArgumentException("Incapacidad no encontrada");

        var pendingStatus = await _context.RequestStatuses
            .FirstAsync(rs => rs.Name == "Pendiente");

        var approvedStatus = await _context.RequestStatuses
            .FirstAsync(rs => rs.Name == "Aprobada");

        var rejectedStatus = await _context.RequestStatuses
            .FirstAsync(rs => rs.Name == "Rechazada");

        if (request.RequestStatusId != pendingStatus.Id)
            throw new InvalidOperationException("Solo se pueden revisar incapacidades pendientes");

        request.RequestStatusId = approve ? approvedStatus.Id : rejectedStatus.Id;
        request.ReviewedById = reviewerUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewComments = comments;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            reviewerUserId,
            approve ? "APPROVE" : "REJECT",
            "INCAPACIDADES",
            "DisabilityRequest",
            id,
            description: $"Incapacidad {(approve ? "aprobada" : "rechazada")}: {request.TotalDays} días");

        if (request.Employee?.User != null)
        {
            var statusText = approve ? "aprobada" : "rechazada";

            await _notificationService.CreateNotificationAsync(
                request.Employee.User.Id,
                $"Incapacidad {statusText}",
                $"Tu incapacidad del {request.StartDate:dd/MM/yyyy} al {request.EndDate:dd/MM/yyyy} ha sido {statusText}. {comments}",
                approve ? "SUCCESS" : "WARNING",
                "INCAPACIDADES",
                "DisabilityRequest",
                id);
        }

        return (await GetByIdAsync(id))!;
    }

    private static DisabilityRequestDto MapToDto(DisabilityRequest dr)
    {
        return new DisabilityRequestDto
        {
            Id = dr.Id,
            EmployeeId = dr.EmployeeId,
            EmployeeName = dr.Employee?.FullName ?? string.Empty,
            StartDate = dr.StartDate,
            EndDate = dr.EndDate,
            TotalDays = dr.TotalDays,
            Type = dr.DisabilityType?.Name ?? string.Empty,
            Diagnosis = dr.Diagnosis,
            DoctorName = dr.DoctorName,
            MedicalCenter = dr.MedicalCenter,
            DocumentNumber = dr.DocumentNumber,
            AttachmentPath = dr.AttachmentPath,
            Status = dr.RequestStatus?.Name ?? string.Empty,
            ReviewedByName = dr.ReviewedBy?.Username,
            ReviewedAt = dr.ReviewedAt,
            ReviewComments = dr.ReviewComments,
            CreatedAt = dr.CreatedAt
        };
    }
}