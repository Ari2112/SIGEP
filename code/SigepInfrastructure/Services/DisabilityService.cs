using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Disability;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepDomain.Enums;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class DisabilityService : IDisabilityService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public DisabilityService(ApplicationDbContext context, IAuditService auditService, INotificationService notificationService)
    {
        _context = context;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<DisabilityRequestDto>> GetAllAsync(DisabilityFilterDto? filter = null)
    {
        var query = _context.DisabilityRequests
            .Include(dr => dr.Employee).ThenInclude(e => e!.Position)
            .Include(dr => dr.ReviewedBy)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
                query = query.Where(dr => dr.EmployeeId == filter.EmployeeId.Value);
            if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<DisabilityStatus>(filter.Status, out var status))
                query = query.Where(dr => dr.Status == status);
            if (filter.DateFrom.HasValue)
                query = query.Where(dr => dr.StartDate >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                query = query.Where(dr => dr.StartDate <= filter.DateTo.Value);
        }

        var requests = await query.OrderByDescending(dr => dr.CreatedAt).ToListAsync();
        return requests.Select(MapToDto);
    }

    public async Task<IEnumerable<DisabilityRequestDto>> GetByEmployeeAsync(int employeeId)
    {
        var requests = await _context.DisabilityRequests
            .Include(dr => dr.Employee).ThenInclude(e => e!.Position)
            .Include(dr => dr.ReviewedBy)
            .Where(dr => dr.EmployeeId == employeeId)
            .OrderByDescending(dr => dr.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    public async Task<DisabilityRequestDto?> GetByIdAsync(int id)
    {
        var dr = await _context.DisabilityRequests
            .Include(d => d.Employee).ThenInclude(e => e!.Position)
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

        var request = new DisabilityRequest
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = totalDays,
            Type = (DisabilityType)dto.Type,
            Diagnosis = dto.Diagnosis,
            DoctorName = dto.DoctorName,
            MedicalCenter = dto.MedicalCenter,
            DocumentNumber = dto.DocumentNumber,
            Status = DisabilityStatus.Pendiente,
            CreatedAt = DateTime.UtcNow
        };

        _context.DisabilityRequests.Add(request);
        await _context.SaveChangesAsync();

        // Notificar a RRHH
        var hrUsers = await _context.Users
            .Where(u => u.Role == UserRole.RRHH || u.Role == UserRole.Admin)
            .ToListAsync();

        foreach (var hrUser in hrUsers)
        {
            await _notificationService.CreateNotificationAsync(
                hrUser.Id,
                "Nueva incapacidad registrada",
                $"{employee.FullName} registró una incapacidad del {dto.StartDate:dd/MM/yyyy} al {dto.EndDate:dd/MM/yyyy} ({totalDays} días).",
                "WARNING", "INCAPACIDADES", "DisabilityRequest", request.Id);
        }

        return (await GetByIdAsync(request.Id))!;
    }

    public async Task<DisabilityRequestDto> ReviewAsync(int id, int reviewerUserId, bool approve, string? comments = null)
    {
        var request = await _context.DisabilityRequests
            .Include(dr => dr.Employee).ThenInclude(e => e!.User)
            .FirstOrDefaultAsync(dr => dr.Id == id)
            ?? throw new ArgumentException("Incapacidad no encontrada");

        if (request.Status != DisabilityStatus.Pendiente)
            throw new InvalidOperationException("Solo se pueden revisar incapacidades pendientes");

        var oldStatus = request.Status;
        request.Status = approve ? DisabilityStatus.Aprobada : DisabilityStatus.Rechazada;
        request.ReviewedById = reviewerUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewComments = comments;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(reviewerUserId, approve ? "APPROVE" : "REJECT", "INCAPACIDADES",
            "DisabilityRequest", id,
            description: $"Incapacidad {(approve ? "aprobada" : "rechazada")}: {request.TotalDays} días");

        // Notificar al empleado
        if (request.Employee?.User != null)
        {
            var statusText = approve ? "aprobada" : "rechazada";
            await _notificationService.CreateNotificationAsync(
                request.Employee.User.Id,
                $"Incapacidad {statusText}",
                $"Tu incapacidad del {request.StartDate:dd/MM/yyyy} al {request.EndDate:dd/MM/yyyy} ha sido {statusText}. {comments}",
                approve ? "SUCCESS" : "WARNING",
                "INCAPACIDADES", "DisabilityRequest", id);
        }

        return (await GetByIdAsync(id))!;
    }

    private static DisabilityRequestDto MapToDto(DisabilityRequest dr) => new()
    {
        Id = dr.Id,
        EmployeeId = dr.EmployeeId,
        EmployeeName = dr.Employee?.FullName ?? string.Empty,
        StartDate = dr.StartDate,
        EndDate = dr.EndDate,
        TotalDays = dr.TotalDays,
        Type = dr.Type.ToString(),
        Diagnosis = dr.Diagnosis,
        DoctorName = dr.DoctorName,
        MedicalCenter = dr.MedicalCenter,
        DocumentNumber = dr.DocumentNumber,
        Status = dr.Status.ToString(),
        ReviewedByName = dr.ReviewedBy?.Username,
        ReviewedAt = dr.ReviewedAt,
        ReviewComments = dr.ReviewComments,
        CreatedAt = dr.CreatedAt
    };
}
