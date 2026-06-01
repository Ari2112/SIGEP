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

    public AttendanceService(
        ApplicationDbContext context,
        IAuditService auditService,
        IOvertimeService overtimeService)
    {
        _context = context;
        _auditService = auditService;
        _overtimeService = overtimeService;
    }

    public async Task<AttendanceRecordDto?> GetTodayRecordAsync(int employeeId)
    {
        var today = TimeZoneInfo.ConvertTimeFromUtc(
    DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica")).Date;

        var record = await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Schedule)
            .Include(a => a.AttendanceStatus)
            .Include(a => a.OvertimeRecords)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

        return record == null ? null : MapToDto(record);
    }

    public async Task<IEnumerable<AttendanceRecordDto>> GetEmployeeRecordsAsync(
        int employeeId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Schedule)
            .Include(a => a.AttendanceStatus)
            .Include(a => a.OvertimeRecords)
            .Where(a => a.EmployeeId == employeeId)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(a => a.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(a => a.Date <= dateTo.Value.Date);

        var records = await query
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<AttendanceRecordDto>> GetAllRecordsAsync(AttendanceFilterDto? filter = null)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Schedule)
            .Include(a => a.AttendanceStatus)
            .Include(a => a.OvertimeRecords)
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

        var records = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee!.LastName)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<AttendanceRecordDto> CheckInAsync(int employeeId, int userId, string? notes = null)
    {
        var employee = await _context.Employees
            .Include(e => e.Schedule)
            .FirstOrDefaultAsync(e => e.Id == employeeId)
            ?? throw new ArgumentException("Empleado no encontrado");

        var today = TimeZoneInfo.ConvertTimeFromUtc(
    DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica")).Date;

        // Validar que no exista ya una entrada hoy
        var existing = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today);

        if (existing != null)
            throw new InvalidOperationException("Ya existe un registro de entrada para el día de hoy");

        var parcialStatus = await GetAttendanceStatusAsync("Parcial");
       var now = TimeZoneInfo.ConvertTimeFromUtc(
    DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica"));

        // Detectar tardía comparando con el horario del empleado
        bool isLate = false;
        int lateMinutes = 0;

        if (employee.Schedule != null)
        {
            // Hora esperada de entrada según horario
            var expectedCheckIn = today.Add(employee.Schedule.StartTime);

            // Tolerancia de 5 minutos
            if (now > expectedCheckIn.AddMinutes(5))
            {
                isLate = true;
                lateMinutes = (int)(now - expectedCheckIn).TotalMinutes;
            }
        }

        var record = new AttendanceRecord
        {
            EmployeeId         = employeeId,
            Date               = today,
            CheckInTime        = now,
            AttendanceStatusId = parcialStatus.Id,
            IsLate             = isLate,
            LateMinutes        = lateMinutes,
            Notes              = notes,
            CreatedAt          = now
        };

        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync();

        var lateMsg = isLate ? $" — TARDÍA: {lateMinutes} minutos" : "";
        await _auditService.LogAsync(
            userId,
            "CHECK_IN",
            "ASISTENCIA",
            "AttendanceRecord",
            record.Id,
            description: $"Entrada registrada a las {now:HH:mm}{lateMsg}"
        );

        return await GetTodayRecordAsync(employeeId)
            ?? throw new Exception("Error al registrar entrada");
    }

    public async Task<AttendanceRecordDto> CheckOutAsync(
        int employeeId,
        int userId,
        string? notes = null,
        string? overtimeReason = null)
    {
       var today = TimeZoneInfo.ConvertTimeFromUtc(
    DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica")).Date;

        var record = await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Schedule)
            .Include(a => a.AttendanceStatus)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == today)
            ?? throw new InvalidOperationException("No existe registro de entrada para el día de hoy");

        if (record.CheckOutTime.HasValue)
            throw new InvalidOperationException("Ya existe un registro de salida para el día de hoy");

        var now = TimeZoneInfo.ConvertTimeFromUtc(
    DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica"));
        var schedule = record.Employee?.Schedule;

        // Verificar si salida es después del horario — exigir motivo
        if (schedule != null)
        {
            var expectedCheckOut = today.Add(schedule.EndTime);

            if (now > expectedCheckOut.AddMinutes(10))
            {
                // Si sale más de 10 min después del horario, el motivo es obligatorio
                if (string.IsNullOrWhiteSpace(overtimeReason))
                    throw new InvalidOperationException(
                        "Debe ingresar el motivo de las horas extra antes de registrar la salida");
            }
        }

        var completoStatus = await GetAttendanceStatusAsync("Completo");

        record.CheckOutTime        = now;
        record.AttendanceStatusId  = completoStatus.Id;
        record.WorkedHours         = record.CheckInTime.HasValue
            ? (decimal)(now - record.CheckInTime.Value).TotalHours
            : 0;

        if (!string.IsNullOrEmpty(notes))
            record.Notes = notes;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId,
            "CHECK_OUT",
            "ASISTENCIA",
            "AttendanceRecord",
            record.Id,
            description: $"Salida registrada a las {now:HH:mm}, horas trabajadas: {record.WorkedHours:F2}"
        );

        // Detectar horas extra automáticamente pasando el motivo
        await _overtimeService.DetectOvertimeFromAttendanceAsync(record.Id, overtimeReason);

        return await GetTodayRecordAsync(employeeId)
            ?? throw new Exception("Error al registrar salida");
    }

    /// <summary>
    /// Reporte de tardías por período
    /// </summary>
    public async Task<IEnumerable<LatenessReportDto>> GetLatenessReportAsync(
        DateTime dateFrom,
        DateTime dateTo,
        int? employeeId = null)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Schedule)
            .Where(a => a.IsLate
                     && a.Date >= dateFrom.Date
                     && a.Date <= dateTo.Date)
            .AsQueryable();

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        var records = await query
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return records
            .Where(a => a.CheckInTime.HasValue && a.Employee?.Schedule != null)
            .Select(a => new LatenessReportDto
            {
                EmployeeId     = a.EmployeeId,
                EmployeeName   = a.Employee?.FullName ?? string.Empty,
                Date           = a.Date,
                ScheduledStart = a.Employee!.Schedule!.StartTime,
                CheckInTime    = a.CheckInTime!.Value,
                LateMinutes    = a.LateMinutes
            });
    }

    /// <summary>
    /// Resumen de asistencia por empleado en un período
    /// </summary>
    public async Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceSummaryAsync(
        DateTime dateFrom,
        DateTime dateTo)
    {
        var records = await _context.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.OvertimeRecords)
            .Where(a => a.Date >= dateFrom.Date && a.Date <= dateTo.Date)
            .ToListAsync();

        return records
            .GroupBy(a => a.EmployeeId)
            .Select(g => new AttendanceSummaryDto
            {
                EmployeeId        = g.Key,
                EmployeeName      = g.First().Employee?.FullName ?? string.Empty,
                TotalDays         = g.Count(),
                PresentDays       = g.Count(a => a.CheckInTime.HasValue),
                AbsentDays        = g.Count(a => !a.CheckInTime.HasValue),
                LateDays          = g.Count(a => a.IsLate),
                TotalWorkedHours  = g.Sum(a => a.WorkedHours ?? 0),
                TotalOvertimeHours = g.SelectMany(a => a.OvertimeRecords).Sum(o => o.TotalHours)
            });
    }

    private async Task<AttendanceStatus> GetAttendanceStatusAsync(string name)
    {
        var status = await _context.AttendanceStatuses
            .FirstOrDefaultAsync(s => s.Name == name)
            ?? throw new InvalidOperationException($"No existe el estado de asistencia: {name}");

        return status;
    }

    private static AttendanceRecordDto MapToDto(AttendanceRecord a)
    {
        var schedule = a.Employee?.Schedule;

        return new AttendanceRecordDto
        {
            Id                  = a.Id,
            EmployeeId          = a.EmployeeId,
            EmployeeName        = a.Employee?.FullName ?? string.Empty,
            Date                = a.Date,
            CheckInTime         = a.CheckInTime,
            CheckOutTime        = a.CheckOutTime,
            WorkedHours         = a.WorkedHours,
            Status              = a.AttendanceStatus?.Name ?? string.Empty,
            Notes               = a.Notes,
            CreatedAt           = a.CreatedAt,
            IsLate              = a.IsLate,
            LateMinutes         = a.LateMinutes,
            ScheduleName        = schedule?.Name,
            ScheduledStartTime  = schedule?.StartTime,
            ScheduledEndTime    = schedule?.EndTime,
            OvertimeHours       = a.OvertimeRecords.Any()
                ? a.OvertimeRecords.Sum(o => o.TotalHours)
                : null
        };
    }
}