using Microsoft.EntityFrameworkCore;
using SigepApplication.DTOs.Reports;
using SigepApplication.Interfaces;
using SigepDomain.Entities;
using SigepDomain.Enums;
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

        var query = _context.Employees
            .Include(e => e.Position)
            .Where(e => e.Status == EmployeeStatus.Activo);

        if (filter.EmployeeId.HasValue)
            query = query.Where(e => e.Id == filter.EmployeeId.Value);

        var employees = await query.ToListAsync();

        var result = new List<AttendanceReportDto>();

        foreach (var emp in employees)
        {
            var records = await _context.AttendanceRecords
                .Where(a => a.EmployeeId == emp.Id
                         && a.Date >= dateFrom.Date
                         && a.Date <= dateTo.Date)
                .ToListAsync();

            int totalDays = (int)(dateTo - dateFrom).TotalDays + 1;
            int presentDays = records.Count(r => r.Status == AttendanceStatus.Completo || r.Status == AttendanceStatus.Parcial);
            int absentDays = records.Count(r => r.Status == AttendanceStatus.Ausente);
            int permissionDays = records.Count(r => r.Status == AttendanceStatus.Permiso);
            int vacationDays = records.Count(r => r.Status == AttendanceStatus.Vacaciones);
            int disabilityDays = records.Count(r => r.Status == AttendanceStatus.Incapacidad);
            decimal totalHours = records.Where(r => r.WorkedHours.HasValue).Sum(r => r.WorkedHours!.Value);
            decimal attendanceRate = totalDays > 0 ? Math.Round((decimal)presentDays / totalDays * 100, 1) : 0;

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

        var query = _context.Employees
            .Include(e => e.Position)
            .Where(e => e.Status == EmployeeStatus.Activo);

        if (filter.EmployeeId.HasValue)
            query = query.Where(e => e.Id == filter.EmployeeId.Value);

        var employees = await query.ToListAsync();
        var result = new List<OvertimeReportDto>();

        foreach (var emp in employees)
        {
            var records = await _context.OvertimeRecords
                .Where(o => o.EmployeeId == emp.Id
                         && o.Date >= dateFrom.Date
                         && o.Date <= dateTo.Date)
                .ToListAsync();

            if (!records.Any()) continue;

            var approved = records.Where(r => r.Status == OvertimeStatus.Aprobada || r.Status == OvertimeStatus.Pagada).ToList();

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
            .Include(p => p.Details)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e!.Position)
            .FirstOrDefaultAsync(p => p.Id == payrollId);

        if (payroll == null) return null;

        return new PayrollSummaryReportDto
        {
            PayrollId = payroll.Id,
            PeriodYear = payroll.PeriodYear,
            PeriodMonth = payroll.PeriodMonth,
            PeriodType = payroll.PeriodType.ToString(),
            Status = payroll.Status.ToString(),
            TotalEmployees = payroll.TotalEmployees,
            TotalGrossSalary = payroll.TotalGrossSalary,
            TotalDeductions = payroll.TotalDeductions,
            TotalBenefits = payroll.TotalBenefits,
            TotalNetSalary = payroll.TotalNetSalary,
            Employees = payroll.Details.Select(d => new PayrollDetailSummaryDto
            {
                EmployeeName = d.Employee?.FullName ?? string.Empty,
                PositionName = d.Employee?.Position?.Name,
                BaseSalary = d.BaseSalary,
                OvertimeAmount = d.OvertimeAmount,
                GrossSalary = d.GrossSalary,
                TotalDeductions = d.TotalDeductions,
                NetSalary = d.NetSalary
            }).OrderBy(d => d.EmployeeName).ToList()
        };
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.Today;

        var totalEmployees = await _context.Employees.CountAsync();
        var activeEmployees = await _context.Employees.CountAsync(e => e.Status == EmployeeStatus.Activo);
        var pendingVacations = await _context.VacationRequests.CountAsync(v => v.Status == RequestStatus.Pendiente);
        var pendingPermissions = await _context.PermissionRequests.CountAsync(p => p.Status == RequestStatus.Pendiente);
        var pendingOvertimes = await _context.OvertimeRecords.CountAsync(o => o.Status == OvertimeStatus.Detectada);
        var pendingDisabilities = await _context.DisabilityRequests.CountAsync(d => d.Status == DisabilityStatus.Pendiente);

        var attendanceToday = await _context.AttendanceRecords.CountAsync(a => a.Date == today.Date);
        var checkedInToday = await _context.AttendanceRecords.CountAsync(a => a.Date == today.Date && a.CheckInTime != null);

        // Total nómina del mes actual (última planilla completada)
        var currentMonth = DateTime.Now;
        var lastPayroll = await _context.Payrolls
            .Where(p => p.PeriodYear == currentMonth.Year && p.PeriodMonth == currentMonth.Month
                     && (p.Status == PayrollStatus.Completada || p.Status == PayrollStatus.Anulada == false))
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
