using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.PerformanceEvaluation;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PerformanceEvaluationController : ControllerBase
{
    private readonly IPerformanceEvaluationService _evaluationService;
    private readonly ILogger<PerformanceEvaluationController> _logger;

    public PerformanceEvaluationController(IPerformanceEvaluationService evaluationService, ILogger<PerformanceEvaluationController> logger)
    {
        _evaluationService = evaluationService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int? GetEmployeeId()
    {
        var claim = User.FindFirstValue("EmployeeId");
        return claim != null ? int.Parse(claim) : null;
    }

    /// <summary>Obtiene todas las evaluaciones con filtros (Admin/RRHH/Jefatura) (HU-8.1)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<IEnumerable<PerformanceEvaluationDto>>> GetAll([FromQuery] EvaluationFilterDto? filter)
    {
        var evals = await _evaluationService.GetAllAsync(filter);
        return Ok(evals);
    }

    /// <summary>Obtiene las evaluaciones del empleado autenticado</summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<PerformanceEvaluationDto>>> GetMy()
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var evals = await _evaluationService.GetByEmployeeAsync(employeeId.Value);
        return Ok(evals);
    }

    /// <summary>Obtiene evaluaciones de un empleado específico (Admin/RRHH/Jefatura)</summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<IEnumerable<PerformanceEvaluationDto>>> GetByEmployee(int employeeId)
    {
        var evals = await _evaluationService.GetByEmployeeAsync(employeeId);
        return Ok(evals);
    }

    /// <summary>Obtiene una evaluación por ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PerformanceEvaluationDto>> GetById(int id)
    {
        var eval = await _evaluationService.GetByIdAsync(id);
        if (eval == null)
            return NotFound(new { message = "Evaluación no encontrada" });

        var role = User.FindFirstValue(ClaimTypes.Role);
        var employeeId = GetEmployeeId();
        if (role != "Admin" && role != "RRHH" && role != "Jefatura" && eval.EmployeeId != employeeId)
            return Forbid();

        return Ok(eval);
    }

    /// <summary>Crea una evaluación de desempeño (HU-8.1)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<PerformanceEvaluationDto>> Create([FromBody] CreateEvaluationDto dto)
    {
        try
        {
            var eval = await _evaluationService.CreateAsync(dto, GetUserId());
            return Ok(eval);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Actualiza una evaluación (HU-8.1)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<PerformanceEvaluationDto>> Update(int id, [FromBody] UpdateEvaluationDto dto)
    {
        try
        {
            var eval = await _evaluationService.UpdateAsync(id, dto, GetUserId());
            return Ok(eval);
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

    /// <summary>El empleado confirma haber revisado su evaluación (HU-8.2)</summary>
    [HttpPost("{id}/acknowledge")]
    public async Task<ActionResult<PerformanceEvaluationDto>> Acknowledge(int id)
    {
        try
        {
            var eval = await _evaluationService.AcknowledgeAsync(id, GetUserId());
            return Ok(eval);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }
}
