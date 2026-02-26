using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Attendance;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IOvertimeService _overtimeService;

    public AttendanceService(ApplicationDbContext context, IAuditService auditService, IOvertimeService overtimeService)
    {
        _context = context;
        _auditService = auditService;
        _overtimeService = overtimeService;
    }

    public async Task<AttendanceRecordDto?> GetTodayRecordAsync(int employeeId)
    {
        var today = DateTime.UtcNow.Date;
        var record = await _context.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

        return record == null ? null : MapToDto(record);
    }

    public async Task<IEnumerable<AttendanceRecordDto>> GetEmployeeRecordsAsync(int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == employeeId)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(a => a.Date >= dateFrom.Value.Date);
        if (dateTo.HasValue)
            query = query.Where(a => a.Date <= dateTo.Value.Date);

        return await query
            .OrderByDescending(a => a.Date)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<IEnumerable<AttendanceRecordDto>> GetAllRecordsAsync(AttendanceFilterDto? filter = null)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
                query = query.Where(a => a.EmployeeId == filter.EmployeeId.Value);
            if (filter.DateFrom.HasValue)
                query = query.Where(a => a.Date >= filter.DateFrom.Value.Date);
            if (filter.DateTo.HasValue)
                query = query.Where(a => a.Date <= filter.DateTo.Value.Date);
        }

        return await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee!.LastName)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<AttendanceRecordDto> CheckInAsync(int employeeId, int userId, string? notes = null)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
            throw new ArgumentException("Empleado no encontrado");

        var today = DateTime.UtcNow.Date;

        var existing = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

        if (existing != null)
            throw new InvalidOperationException("Ya existe un registro de entrada para el día de hoy");

        var now = DateTime.UtcNow;
        var record = new AttendanceRecord
        {
            EmployeeId = employeeId,
            Date = today,
            CheckInTime = now,
            Status = AttendanceStatus.Parcial,
            Notes = notes,
            CreatedAt = now
        };

        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(userId, "CHECK_IN", "ASISTENCIA", "AttendanceRecord", record.Id,
            description: $"Entrada registrada a las {now:HH:mm}");

        return await GetTodayRecordAsync(employeeId) ?? throw new Exception("Error al registrar entrada");
    }

    public async Task<AttendanceRecordDto> CheckOutAsync(int employeeId, int userId, string? notes = null)
    {
        var today = DateTime.UtcNow.Date;
        var record = await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Schedule)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

        if (record == null)
            throw new InvalidOperationException("No existe registro de entrada para el día de hoy");

        if (record.CheckOutTime.HasValue)
            throw new InvalidOperationException("Ya existe un registro de salida para el día de hoy");

        var now = DateTime.UtcNow;
        record.CheckOutTime = now;
        record.Status = AttendanceStatus.Completo;

        if (record.CheckInTime.HasValue)
            record.WorkedHours = (decimal)(now - record.CheckInTime.Value).TotalHours;

        if (!string.IsNullOrEmpty(notes))
            record.Notes = notes;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(userId, "CHECK_OUT", "ASISTENCIA", "AttendanceRecord", record.Id,
            description: $"Salida registrada a las {now:HH:mm}, horas trabajadas: {record.WorkedHours:F2}");

        // Detectar horas extra automáticamente
        await _overtimeService.DetectOvertimeFromAttendanceAsync(record.Id);

        return MapToDto(record);
    }

    private static AttendanceRecordDto MapToDto(AttendanceRecord a)
    {
        return new AttendanceRecordDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee?.FullName ?? string.Empty,
            Date = a.Date,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            WorkedHours = a.WorkedHours,
            Status = a.Status.ToString(),
            Notes = a.Notes,
            CreatedAt = a.CreatedAt
        };
    }
}
