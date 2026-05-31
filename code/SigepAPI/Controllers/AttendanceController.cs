using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Attendance;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int? GetEmployeeId()
    {
        var claim = User.FindFirstValue("EmployeeId");
        return claim != null ? int.Parse(claim) : null;
    }

    /// <summary>
    /// Obtiene el registro de asistencia de hoy del empleado actual
    /// </summary>
    [HttpGet("today")]
    public async Task<ActionResult<AttendanceRecordDto?>> GetToday()
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var record = await _attendanceService.GetTodayRecordAsync(employeeId.Value);
        return Ok(record);
    }

    /// <summary>
    /// Obtiene el historial de asistencia del empleado actual
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<AttendanceRecordDto>>> GetMyRecords(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var records = await _attendanceService.GetEmployeeRecordsAsync(employeeId.Value, dateFrom, dateTo);
        return Ok(records);
    }

    /// <summary>
    /// Obtiene registros de asistencia de un empleado específico (Admin/RRHH)
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Admin,Administrador,RRHH,Jefatura")]
    public async Task<ActionResult<IEnumerable<AttendanceRecordDto>>> GetByEmployee(
        int employeeId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var records = await _attendanceService.GetEmployeeRecordsAsync(employeeId, dateFrom, dateTo);
        return Ok(records);
    }

    /// <summary>
    /// Obtiene todos los registros con filtros opcionales (Admin/RRHH)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Administrador,RRHH,Jefatura")]
    public async Task<ActionResult<IEnumerable<AttendanceRecordDto>>> GetAll([FromQuery] AttendanceFilterDto? filter)
    {
        var records = await _attendanceService.GetAllRecordsAsync(filter);
        return Ok(records);
    }

    /// <summary>
    /// Registra la entrada del empleado actual (HU-3.1)
    /// </summary>
    [HttpPost("check-in")]
    public async Task<ActionResult<AttendanceRecordDto>> CheckIn([FromBody] CheckInDto? dto)
    {
        try
        {
            var employeeId = GetEmployeeId();
            if (!employeeId.HasValue)
                return BadRequest(new { message = "Usuario no tiene empleado asociado" });

            var record = await _attendanceService.CheckInAsync(employeeId.Value, GetUserId(), dto?.Notes);
            return Ok(record);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registra la salida del empleado actual (HU-3.1)
    /// </summary>
[HttpPost("check-out")]
public async Task<ActionResult<AttendanceRecordDto>> CheckOut([FromBody] CheckOutDto? dto)
{
    try
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var record = await _attendanceService.CheckOutAsync(
            employeeId.Value,
            GetUserId(),
            dto?.Notes,
            dto?.OvertimeReason);

        return Ok(record);
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

/// <summary>
/// Reporte de tardías por período (Admin/RRHH)
/// </summary>
[HttpGet("report/lateness")]
[Authorize(Roles = "Admin,Administrador,RRHH,Jefatura")]
public async Task<ActionResult<IEnumerable<LatenessReportDto>>> GetLatenessReport(
    [FromQuery] DateTime dateFrom,
    [FromQuery] DateTime dateTo,
    [FromQuery] int? employeeId = null)
{
    var report = await _attendanceService.GetLatenessReportAsync(dateFrom, dateTo, employeeId);
    return Ok(report);
}

/// <summary>
/// Resumen de asistencia por empleado (Admin/RRHH)
/// </summary>
[HttpGet("report/summary")]
[Authorize(Roles = "Admin,Administrador,RRHH,Jefatura")]
public async Task<ActionResult<IEnumerable<AttendanceSummaryDto>>> GetAttendanceSummary(
    [FromQuery] DateTime dateFrom,
    [FromQuery] DateTime dateTo)
{
    var summary = await _attendanceService.GetAttendanceSummaryAsync(dateFrom, dateTo);
    return Ok(summary);
}
    }
