using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Reports;
using SigepApplication.Interfaces;
using SigepInfrastructure.Persistence;

namespace SigepInfrastructure.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(ReportFilterDto filter)
    {
        var dateFrom = filter.DateFrom ?? DateTime.Now.AddMonths(-1);
        var dateTo = filter.DateTo ?? DateTime.Now;

        var activeStatus = await _context.EmployeeStatuses
            .FirstAsync(es => es.Name == "Activo");

        var query = _context.Employees
            .Include(e => e.Position)
            .Where(e => e.EmployeeStatusId == activeStatus.Id);

        if (filter.EmployeeId.HasValue)
        {
            query = query.Where(e => e.Id == filter.EmployeeId.Value);
        }

        var employees = await query.ToListAsync();

        var result = new List<AttendanceReportDto>();

        foreach (var emp in employees)
        {
            var records = await _context.AttendanceRecords
                .Include(a => a.AttendanceStatus)
                .Where(a =>
                    a.EmployeeId == emp.Id &&
                    a.Date >= dateFrom.Date &&
                    a.Date <= dateTo.Date)
                .ToListAsync();

            int totalDays = (int)(dateTo.Date - dateFrom.Date).TotalDays + 1;

            int presentDays = records.Count(r =>
                r.AttendanceStatus != null &&
                (r.AttendanceStatus.Name == "Completo" ||
                 r.AttendanceStatus.Name == "Parcial"));

            int absentDays = records.Count(r =>
                r.AttendanceStatus != null &&
                r.AttendanceStatus.Name == "Ausente");

            int permissionDays = records.Count(r =>
                r.AttendanceStatus != null &&
                r.AttendanceStatus.Name == "Permiso");

            int vacationDays = records.Count(r =>
                r.AttendanceStatus != null &&
                r.AttendanceStatus.Name == "Vacaciones");

            int disabilityDays = records.Count(r =>
                r.AttendanceStatus != null &&
                r.AttendanceStatus.Name == "Incapacidad");

            decimal totalHours = records
                .Where(r => r.WorkedHours.HasValue)
                .Sum(r => r.WorkedHours!.Value);

            decimal attendanceRate = totalDays > 0
                ? Math.Round((decimal)presentDays / totalDays * 100, 1)
                : 0;

            result.Add(new AttendanceReportDto
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.FullName,
                PositionName = emp.Position?.Name,
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                PermissionDays = permissionDays,
                VacationDays = vacationDays,
                DisabilityDays = disabilityDays,
                TotalWorkedHours = totalHours,
                AttendanceRate = attendanceRate
            });
        }

        return result.OrderBy(r => r.EmployeeName);
    }

    public async Task<IEnumerable<OvertimeReportDto>> GetOvertimeReportAsync(ReportFilterDto filter)
    {
        var dateFrom = filter.DateFrom ?? DateTime.Now.AddMonths(-1);
        var dateTo = filter.DateTo ?? DateTime.Now;

        var activeStatus = await _context.EmployeeStatuses
            .FirstAsync(es => es.Name == "Activo");

        var query = _context.Employees
            .Include(e => e.Position)
            .Where(e => e.EmployeeStatusId == activeStatus.Id);

        if (filter.EmployeeId.HasValue)
        {
            query = query.Where(e => e.Id == filter.EmployeeId.Value);
        }

        var employees = await query.ToListAsync();

        var result = new List<OvertimeReportDto>();

        foreach (var emp in employees)
        {
            var records = await _context.OvertimeRecords
                .Where(o =>
                    o.EmployeeId == emp.Id &&
                    o.Date >= dateFrom.Date &&
                    o.Date <= dateTo.Date)
                .ToListAsync();

            if (!records.Any())
            {
                continue;
            }

            var approved = records
                .Where(r =>
                    r.Status.ToString() == "Aprobada" ||
                    r.Status.ToString() == "Pagada")
                .ToList();

            result.Add(new OvertimeReportDto
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.FullName,
                PositionName = emp.Position?.Name,
                TotalRecords = records.Count,
                TotalHours = records.Sum(r => r.TotalHours),
                TotalAmount = records.Sum(r => r.TotalAmount),
                ApprovedRecords = approved.Count,
                ApprovedHours = approved.Sum(r => r.TotalHours),
                ApprovedAmount = approved.Sum(r => r.TotalAmount)
            });
        }

        return result.OrderByDescending(r => r.TotalHours);
    }

    public async Task<PayrollSummaryReportDto?> GetPayrollReportAsync(int payrollId)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.PayrollStatus)
            .Include(p => p.PayrollPeriodType)
            .Include(p => p.Details)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e!.Position)
            .FirstOrDefaultAsync(p => p.Id == payrollId);

        if (payroll == null)
        {
            return null;
        }

        return new PayrollSummaryReportDto
        {
            PayrollId = payroll.Id,
            PeriodYear = payroll.PeriodYear,
            PeriodMonth = payroll.PeriodMonth,
            PeriodType = payroll.PayrollPeriodType?.Name ?? string.Empty,
            Status = payroll.PayrollStatus?.Name ?? string.Empty,
            TotalEmployees = payroll.TotalEmployees,
            TotalGrossSalary = payroll.TotalGrossSalary,
            TotalDeductions = payroll.TotalDeductions,
            TotalBenefits = payroll.TotalBenefits,
            TotalNetSalary = payroll.TotalNetSalary,
            Employees = payroll.Details
                .Select(d => new PayrollDetailSummaryDto
                {
                    EmployeeName = d.Employee?.FullName ?? string.Empty,
                    PositionName = d.Employee?.Position?.Name,
                    BaseSalary = d.BaseSalary,
                    OvertimeAmount = d.OvertimeAmount,
                    GrossSalary = d.GrossSalary,
                    TotalDeductions = d.TotalDeductions,
                    NetSalary = d.NetSalary
                })
                .OrderBy(d => d.EmployeeName)
                .ToList()
        };
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.Today;

        var activeEmployeeStatus = await _context.EmployeeStatuses
            .FirstAsync(es => es.Name == "Activo");

        var pendingRequestStatus = await _context.RequestStatuses
            .FirstAsync(rs => rs.Name == "Pendiente");

        var annulledPayrollStatus = await _context.PayrollStatuses
            .FirstAsync(ps => ps.Name == "Anulada");

        var totalEmployees = await _context.Employees.CountAsync();

        var activeEmployees = await _context.Employees
            .CountAsync(e => e.EmployeeStatusId == activeEmployeeStatus.Id);

        var pendingVacations = await _context.VacationRequests
            .CountAsync(v => v.RequestStatusId == pendingRequestStatus.Id);

        var pendingPermissions = await _context.PermissionRequests
            .CountAsync(p => p.RequestStatusId == pendingRequestStatus.Id);

        var pendingDisabilities = await _context.DisabilityRequests
            .CountAsync(d => d.RequestStatusId == pendingRequestStatus.Id);

        var pendingOvertimes = await _context.OvertimeRecords
            .CountAsync(o => o.Status.ToString() == "Detectada");

        var attendanceToday = await _context.AttendanceRecords
            .CountAsync(a => a.Date == today.Date);

        var checkedInToday = await _context.AttendanceRecords
            .CountAsync(a => a.Date == today.Date && a.CheckInTime != null);

        var currentMonth = DateTime.Now;

        var lastPayroll = await _context.Payrolls
            .Include(p => p.PayrollStatus)
            .Where(p =>
                p.PeriodYear == currentMonth.Year &&
                p.PeriodMonth == currentMonth.Month &&
                p.PayrollStatusId != annulledPayrollStatus.Id)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        return new DashboardStatsDto
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            PendingVacations = pendingVacations,
            PendingPermissions = pendingPermissions,
            PendingOvertimes = pendingOvertimes,
            PendingDisabilities = pendingDisabilities,
            MonthlyPayrollTotal = lastPayroll?.TotalNetSalary ?? 0,
            AttendanceTodayCount = attendanceToday,
            CheckedInToday = checkedInToday
        };
    }
}
