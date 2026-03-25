using SigepApplication.DTOs.Reports;

namespace SigepApplication.Interfaces;

public interface IReportService
{
    Task<IEnumerable<AttendanceReportDto>> GetAttendanceReportAsync(ReportFilterDto filter);
    Task<IEnumerable<OvertimeReportDto>> GetOvertimeReportAsync(ReportFilterDto filter);
    Task<PayrollSummaryReportDto?> GetPayrollReportAsync(int payrollId);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
