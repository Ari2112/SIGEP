using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Reports;
using SigepApplication.Interfaces;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>Estadísticas del dashboard</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _reportService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    /// <summary>Reporte de asistencia por período (HU-10.1)</summary>
    [HttpGet("attendance")]
    [Authorize(Roles = "Admin,Administrador,RRHH")]
    public async Task<ActionResult<IEnumerable<AttendanceReportDto>>> GetAttendanceReport([FromQuery] ReportFilterDto filter)
    {
        var report = await _reportService.GetAttendanceReportAsync(filter);
        return Ok(report);
    }

    /// <summary>Reporte de horas extra por período (HU-10.2)</summary>
    [HttpGet("overtime")]
    [Authorize(Roles = "Admin,Administrador,RRHH")]
    public async Task<ActionResult<IEnumerable<OvertimeReportDto>>> GetOvertimeReport([FromQuery] ReportFilterDto filter)
    {
        var report = await _reportService.GetOvertimeReportAsync(filter);
        return Ok(report);
    }

    /// <summary>Reporte consolidado de planilla (HU-10.3)</summary>
    [HttpGet("payroll/{payrollId}")]
   [Authorize(Roles = "Admin,Administrador,RRHH")]
    public async Task<ActionResult<PayrollSummaryReportDto>> GetPayrollReport(int payrollId)
    {
        var report = await _reportService.GetPayrollReportAsync(payrollId);
        if (report == null)
            return NotFound(new { message = "Planilla no encontrada" });
        return Ok(report);
    }
}
