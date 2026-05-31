using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Settlement;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Administrador,RRHH")]
public class SettlementController : ControllerBase
{
    private readonly ISettlementService _settlementService;
    private readonly ILogger<SettlementController> _logger;

    public SettlementController(ISettlementService settlementService, ILogger<SettlementController> logger)
    {
        _settlementService = settlementService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Obtiene todas las liquidaciones (HU-6.1)</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SettlementDto>>> GetAll()
    {
        var settlements = await _settlementService.GetAllAsync();
        return Ok(settlements);
    }

    /// <summary>Obtiene una liquidación por ID</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SettlementDto>> GetById(int id)
    {
        var settlement = await _settlementService.GetByIdAsync(id);
        if (settlement == null)
            return NotFound(new { message = "Liquidación no encontrada" });
        return Ok(settlement);
    }

    /// <summary>Obtiene liquidación por empleado</summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<SettlementDto>> GetByEmployee(int employeeId)
    {
        var settlement = await _settlementService.GetByEmployeeAsync(employeeId);
        if (settlement == null)
            return NotFound(new { message = "No hay liquidación para este empleado" });
        return Ok(settlement);
    }

    /// <summary>Calcula una liquidación para un empleado (HU-6.1)</summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<SettlementDto>> Calculate([FromBody] CalculateSettlementDto dto)
    {
        try
        {
            var settlement = await _settlementService.CalculateAsync(dto, GetUserId());
            return Ok(settlement);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando liquidación");
            return StatusCode(500, new { message = ex.Message, type = ex.GetType().Name });
        }
    }

    /// <summary>Aprueba una liquidación (HU-6.2)</summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<SettlementDto>> Approve(int id, [FromBody] ApproveSettlementDto dto)
    {
        try
        {
            var settlement = await _settlementService.ApproveAsync(id, GetUserId(), dto.Notes);
            return Ok(settlement);
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

    /// <summary>Marca una liquidación como pagada y liquida al empleado (HU-6.3)</summary>
    [HttpPost("{id}/pay")]
    [Authorize(Roles = "Admin,Administrador")]
    public async Task<ActionResult<SettlementDto>> MarkAsPaid(int id)
    {
        try
        {
            var settlement = await _settlementService.MarkAsPaidAsync(id, GetUserId());
            return Ok(settlement);
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
