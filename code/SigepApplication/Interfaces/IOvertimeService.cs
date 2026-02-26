using SigepApplication.DTOs.Overtime;

namespace SigepApplication.Interfaces;

public interface IOvertimeService
{
    Task<IEnumerable<OvertimeRecordDto>> GetAllAsync(OvertimeFilterDto? filter = null);
    Task<IEnumerable<OvertimeRecordDto>> GetByEmployeeAsync(int employeeId, OvertimeFilterDto? filter = null);
    Task<OvertimeRecordDto?> GetByIdAsync(int id);
    Task<OvertimeRecordDto> ReviewAsync(int id, int reviewerUserId, bool approve, string? comments = null);
    Task DetectOvertimeFromAttendanceAsync(int attendanceId);
}
