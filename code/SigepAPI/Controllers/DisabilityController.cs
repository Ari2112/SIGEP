using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Disability;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DisabilityController : ControllerBase
{
    private readonly IDisabilityService _disabilityService;
    private readonly ILogger<DisabilityController> _logger;

    public DisabilityController(IDisabilityService disabilityService, ILogger<DisabilityController> logger)
    {
        _disabilityService = disabilityService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int? GetEmployeeId()
    {
        var claim = User.FindFirstValue("EmployeeId");
        return claim != null ? int.Parse(claim) : null;
    }

    /// <summary>Obtiene todas las incapacidades con filtros (Admin/RRHH) (HU-9.1)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<ActionResult<IEnumerable<DisabilityRequestDto>>> GetAll([FromQuery] DisabilityFilterDto? filter)
    {
        var requests = await _disabilityService.GetAllAsync(filter);
        return Ok(requests);
    }

    /// <summary>Obtiene las incapacidades del empleado autenticado</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<DisabilityRequestDto>>> GetMy()
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var requests = await _disabilityService.GetByEmployeeAsync(employeeId.Value);
        return Ok(requests);
    }

    /// <summary>Obtiene incapacidades de un empleado específico (Admin/RRHH)</summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<ActionResult<IEnumerable<DisabilityRequestDto>>> GetByEmployee(int employeeId)
    {
        var requests = await _disabilityService.GetByEmployeeAsync(employeeId);
        return Ok(requests);
    }

    /// <summary>Obtiene una incapacidad por ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DisabilityRequestDto>> GetById(int id)
    {
        var request = await _disabilityService.GetByIdAsync(id);
        if (request == null)
            return NotFound(new { message = "Incapacidad no encontrada" });

        var role = User.FindFirstValue(ClaimTypes.Role);
        var employeeId = GetEmployeeId();
        if (role != "Admin" && role != "RRHH" && request.EmployeeId != employeeId)
            return Forbid();

        return Ok(request);
    }

    /// <summary>Registra una incapacidad (empleado) (HU-9.1)</summary>
    [HttpPost]
    public async Task<ActionResult<DisabilityRequestDto>> Create([FromBody] CreateDisabilityDto dto)
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        try
        {
            var request = await _disabilityService.CreateAsync(employeeId.Value, dto);
            return Ok(request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Admin/RRHH puede registrar incapacidad para un empleado específico (HU-9.2)</summary>
    [HttpPost("employee/{employeeId}")]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<ActionResult<DisabilityRequestDto>> CreateForEmployee(int employeeId, [FromBody] CreateDisabilityDto dto)
    {
        try
        {
            var request = await _disabilityService.CreateAsync(employeeId, dto);
            return Ok(request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Aprueba o rechaza una incapacidad (Admin/RRHH) (HU-9.3)</summary>
    [HttpPost("{id}/review")]
    [Authorize(Roles = "Admin,RRHH")]
    public async Task<ActionResult<DisabilityRequestDto>> Review(int id, [FromBody] ReviewDisabilityDto dto)
    {
        try
        {
            var request = await _disabilityService.ReviewAsync(id, GetUserId(), dto.Approve, dto.Comments);
            return Ok(request);
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
