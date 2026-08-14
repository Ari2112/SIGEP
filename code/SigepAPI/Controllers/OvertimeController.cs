using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Overtime;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OvertimeController : ControllerBase
{
    private readonly IOvertimeService _overtimeService;
    private readonly ILogger<OvertimeController> _logger;

    public OvertimeController(IOvertimeService overtimeService, ILogger<OvertimeController> logger)
    {
        _overtimeService = overtimeService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int? GetEmployeeId()
    {
        var claim = User.FindFirstValue("EmployeeId");
        return claim != null ? int.Parse(claim) : null;
    }

    /// <summary>
    /// Obtiene todas las horas extra con filtros (Admin/RRHH) (HU-4.1, HU-4.4)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Administrador,RRHH,Recursos Humanos,Jefatura")]
    public async Task<ActionResult<IEnumerable<OvertimeRecordDto>>> GetAll([FromQuery] OvertimeFilterDto? filter)
    {
        var records = await _overtimeService.GetAllAsync(filter);
        return Ok(records);
    }

    /// <summary>
    /// Obtiene las horas extra del empleado actual
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<OvertimeRecordDto>>> GetMy([FromQuery] OvertimeFilterDto? filter)
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var records = await _overtimeService.GetByEmployeeAsync(employeeId.Value, filter);
        return Ok(records);
    }

    /// <summary>
    /// Obtiene las horas extra de un empleado específico (Admin/RRHH)
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Admin,Administrador,RRHH,Recursos Humanos,Jefatura")]
    public async Task<ActionResult<IEnumerable<OvertimeRecordDto>>> GetByEmployee(
        int employeeId,
        [FromQuery] OvertimeFilterDto? filter)
    {
        var records = await _overtimeService.GetByEmployeeAsync(employeeId, filter);
        return Ok(records);
    }

    /// <summary>
    /// Obtiene un registro por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OvertimeRecordDto>> GetById(int id)
    {
        var record = await _overtimeService.GetByIdAsync(id);
        if (record == null)
            return NotFound(new { message = "Registro no encontrado" });

        var employeeId = GetEmployeeId();
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Admin" && role != "RRHH" && record.EmployeeId != employeeId)
            return Forbid();

        return Ok(record);
    }

    /// <summary>
    /// Permite al empleado registrar o actualizar la justificación de sus horas extra
    /// antes de que sean revisadas por Admin/RRHH
    /// </summary>
    [HttpPost("{id}/justify")]
    public async Task<ActionResult<OvertimeRecordDto>> Justify(int id, [FromBody] JustifyOvertimeDto dto)
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        try
        {
            var record = await _overtimeService.JustifyAsync(id, employeeId.Value, dto.Justification);
            return Ok(record);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Aprueba o rechaza horas extra (Admin/RRHH) (HU-4.2)
    /// </summary>
    [HttpPost("{id}/review")]
    [Authorize(Roles = "Admin,Administrador,RRHH,Recursos Humanos,Jefatura")]
    public async Task<ActionResult<OvertimeRecordDto>> Review(int id, [FromBody] ReviewOvertimeDto dto)
    {
        try
        {
            var record = await _overtimeService.ReviewAsync(id, GetUserId(), dto.Approve, dto.Comments);
            return Ok(record);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}