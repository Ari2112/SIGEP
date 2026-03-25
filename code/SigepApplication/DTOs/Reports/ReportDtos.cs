namespace SigepApplication.DTOs.Reports;

public class AttendanceReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int PermissionDays { get; set; }
    public int VacationDays { get; set; }
    public int DisabilityDays { get; set; }
    public decimal TotalWorkedHours { get; set; }
    public decimal AttendanceRate { get; set; }
}

public class OvertimeReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public int TotalRecords { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
    public int ApprovedRecords { get; set; }
    public decimal ApprovedHours { get; set; }
    public decimal ApprovedAmount { get; set; }
}

public class PayrollSummaryReportDto
{
    public int PayrollId { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public string PeriodType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalBenefits { get; set; }
    public decimal TotalNetSalary { get; set; }
    public List<PayrollDetailSummaryDto> Employees { get; set; } = new();
}

public class PayrollDetailSummaryDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string? PositionName { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
}

public class ReportFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
}

public class DashboardStatsDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PendingVacations { get; set; }
    public int PendingPermissions { get; set; }
    public int PendingOvertimes { get; set; }
    public int PendingDisabilities { get; set; }
    public decimal MonthlyPayrollTotal { get; set; }
    public int AttendanceTodayCount { get; set; }
    public int CheckedInToday { get; set; }
}
