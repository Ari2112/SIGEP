using SigepApplication.DTOs.Attendance;

namespace SigepApplication.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceRecordDto?> GetTodayRecordAsync(int employeeId);
    Task<IEnumerable<AttendanceRecordDto>> GetEmployeeRecordsAsync(int employeeId, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<IEnumerable<AttendanceRecordDto>> GetAllRecordsAsync(AttendanceFilterDto? filter = null);
    Task<AttendanceRecordDto> CheckInAsync(int employeeId, int userId, string? notes = null);
    Task<AttendanceRecordDto> CheckOutAsync(int employeeId, int userId, string? notes = null, string? overtimeReason = null);
    Task<IEnumerable<LatenessReportDto>> GetLatenessReportAsync(DateTime dateFrom, DateTime dateTo, int? employeeId = null);
    Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceSummaryAsync(DateTime dateFrom, DateTime dateTo);
}