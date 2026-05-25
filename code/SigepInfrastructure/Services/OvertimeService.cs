using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Overtime;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class OvertimeService : IOvertimeService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;

    public OvertimeService(ApplicationDbContext context, IAuditService auditService, INotificationService notificationService)
    {
        _context = context;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<OvertimeRecordDto>> GetAllAsync(OvertimeFilterDto? filter = null)
    {
        var query = _context.OvertimeRecords
            .Include(o => o.Employee)
            .Include(o => o.ReviewedBy)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.EmployeeId.HasValue)
                query = query.Where(o => o.EmployeeId == filter.EmployeeId.Value);
            if (filter.DateFrom.HasValue)
                query = query.Where(o => o.Date >= filter.DateFrom.Value.Date);
            if (filter.DateTo.HasValue)
                query = query.Where(o => o.Date <= filter.DateTo.Value.Date);
            if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<OvertimeStatus>(filter.Status, out var status))
                query = query.Where(o => o.Status == status);
        }

        return await query
            .OrderByDescending(o => o.Date)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<IEnumerable<OvertimeRecordDto>> GetByEmployeeAsync(int employeeId, OvertimeFilterDto? filter = null)
    {
        var query = _context.OvertimeRecords
            .Include(o => o.Employee)
            .Include(o => o.ReviewedBy)
            .Where(o => o.EmployeeId == employeeId)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.DateFrom.HasValue)
                query = query.Where(o => o.Date >= filter.DateFrom.Value.Date);
            if (filter.DateTo.HasValue)
                query = query.Where(o => o.Date <= filter.DateTo.Value.Date);
            if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<OvertimeStatus>(filter.Status, out var status))
                query = query.Where(o => o.Status == status);
        }

        return await query
            .OrderByDescending(o => o.Date)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<OvertimeRecordDto?> GetByIdAsync(int id)
    {
        var record = await _context.OvertimeRecords
            .Include(o => o.Employee)
            .Include(o => o.ReviewedBy)
            .FirstOrDefaultAsync(o => o.Id == id);

        return record == null ? null : MapToDto(record);
    }

    public async Task<OvertimeRecordDto> ReviewAsync(int id, int reviewerUserId, bool approve, string? comments = null)
    {
        var record = await _context.OvertimeRecords
            .Include(o => o.Employee)
                .ThenInclude(e => e!.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (record == null)
            throw new ArgumentException("Registro de horas extra no encontrado");

        if (record.Status != OvertimeStatus.Detectada && record.Status != OvertimeStatus.Pendiente)
            throw new InvalidOperationException("Solo se pueden revisar registros detectados o pendientes");

        var oldStatus = record.Status;
        record.Status = approve ? OvertimeStatus.Aprobada : OvertimeStatus.Rechazada;
        record.ReviewedById = reviewerUserId;
        record.ReviewedAt = DateTime.UtcNow;
        record.ReviewComments = comments;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var action = approve ? "APPROVE" : "REJECT";
        await _auditService.LogAsync(reviewerUserId, action, "HORAS_EXTRA", "OvertimeRecord", id,
            oldValues: new { Status = oldStatus.ToString() },
            newValues: new { Status = record.Status.ToString(), Comments = comments },
            description: $"Horas extra {(approve ? "aprobadas" : "rechazadas")}: {record.TotalHours}h del {record.Date:dd/MM/yyyy}");

        if (record.Employee?.User != null)
        {
            var statusText = approve ? "aprobadas" : "rechazadas";
            await _notificationService.CreateNotificationAsync(
                record.Employee.User.Id,
                $"Horas extra {statusText}",
                $"Tus horas extra del {record.Date:dd/MM/yyyy} ({record.TotalHours}h) han sido {statusText}. {comments}",
                approve ? "SUCCESS" : "WARNING",
                "HORAS_EXTRA",
                "OvertimeRecord",
                id);
        }

        return MapToDto(record);
    }

    public async Task DetectOvertimeFromAttendanceAsync(int attendanceId)
    {
        var attendance = await _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Schedule)
            .FirstOrDefaultAsync(a => a.Id == attendanceId);

        if (attendance == null || !attendance.CheckInTime.HasValue || !attendance.CheckOutTime.HasValue)
            return;

        var schedule = attendance.Employee?.Schedule;
        if (schedule == null)
            return;

        var scheduledEndTime = attendance.Date.Add(schedule.EndTime);
        var actualCheckOut = attendance.CheckOutTime.Value;

        if (actualCheckOut > scheduledEndTime)
        {
            var overtimeStart = scheduledEndTime.TimeOfDay;
            var overtimeEnd   = actualCheckOut.TimeOfDay;
            var overtimeHours = (decimal)(actualCheckOut - scheduledEndTime).TotalHours;

            if (overtimeHours < 0.5m)
                return;

            var existingOvertime = await _context.OvertimeRecords
                .FirstOrDefaultAsync(o => o.AttendanceId == attendanceId);

            if (existingOvertime != null)
                return;

            var employee  = attendance.Employee!;
            var hourlyRate = employee.BaseSalary / 240m;

            // Determinar multiplicador según Art. 140 Código de Trabajo CR:
            // Horas extra diurnas: 1.5x
            // Horas extra nocturnas (7pm - 5am) o en feriado: 2.0x
            bool isFeriado = await _context.PublicHolidays
                .AnyAsync(h => h.Date.Date == attendance.Date.Date && h.IsActive);

            bool isNocturna = overtimeStart >= new TimeSpan(19, 0, 0)
                           || overtimeStart < new TimeSpan(5, 0, 0);

            decimal multiplier = (isFeriado || isNocturna) ? 2.0m : 1.5m;

            var overtimeRecord = new OvertimeRecord
            {
                EmployeeId     = attendance.EmployeeId,
                AttendanceId   = attendanceId,
                Date           = attendance.Date,
                StartTime      = overtimeStart,
                EndTime        = overtimeEnd,
                TotalHours     = Math.Round(overtimeHours, 2),
                HourlyRate     = Math.Round(hourlyRate, 2),
                MultiplierRate = multiplier,
                TotalAmount    = Math.Round(hourlyRate * multiplier * overtimeHours, 2),
                Status         = OvertimeStatus.Detectada,
                DetectionType  = OvertimeDetectionType.Automatica,
                CreatedAt      = DateTime.UtcNow
            };

            _context.OvertimeRecords.Add(overtimeRecord);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Determina el multiplicador de horas extra según Art. 140 Código de Trabajo CR.
    /// Diurnas: 1.5x | Nocturnas (7pm-5am) o feriados: 2.0x
    /// </summary>
    private static decimal GetOvertimeMultiplier(TimeSpan startTime, bool isFeriado)
    {
        if (isFeriado) return 2.0m;
        bool isNocturna = startTime >= new TimeSpan(19, 0, 0)
                       || startTime < new TimeSpan(5, 0, 0);
        return isNocturna ? 2.0m : 1.5m;
    }

    private static OvertimeRecordDto MapToDto(OvertimeRecord o)
    {
        return new OvertimeRecordDto
        {
            Id             = o.Id,
            EmployeeId     = o.EmployeeId,
            EmployeeName   = o.Employee?.FullName ?? string.Empty,
            AttendanceId   = o.AttendanceId,
            Date           = o.Date,
            StartTime      = o.StartTime.ToString(@"hh\:mm"),
            EndTime        = o.EndTime.ToString(@"hh\:mm"),
            TotalHours     = o.TotalHours,
            HourlyRate     = o.HourlyRate,
            MultiplierRate = o.MultiplierRate,
            TotalAmount    = o.TotalAmount,
            Status         = o.Status.ToString(),
            DetectionType  = o.DetectionType.ToString(),
            ReviewedByName = o.ReviewedBy?.Username,
            ReviewedAt     = o.ReviewedAt,
            ReviewComments = o.ReviewComments,
            CreatedAt      = o.CreatedAt
        };
    }
}