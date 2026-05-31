using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.AnnualBonus;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Administrador,RRHH")]
public class AnnualBonusController : ControllerBase
{
    private readonly IAnnualBonusService _annualBonusService;
    private readonly ILogger<AnnualBonusController> _logger;

    public AnnualBonusController(IAnnualBonusService annualBonusService, ILogger<AnnualBonusController> logger)
    {
        _annualBonusService = annualBonusService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Obtiene todos los aguinaldos (HU-7.1)</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AnnualBonusDto>>> GetAll()
    {
        var bonuses = await _annualBonusService.GetAllAsync();
        return Ok(bonuses);
    }

    /// <summary>Obtiene un aguinaldo por ID con detalles (HU-7.3)</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AnnualBonusDto>> GetById(int id)
    {
        var bonus = await _annualBonusService.GetByIdAsync(id);
        if (bonus == null)
            return NotFound(new { message = "Aguinaldo no encontrado" });
        return Ok(bonus);
    }

    /// <summary>Obtiene aguinaldo por año</summary>
    [HttpGet("year/{year}")]
    public async Task<ActionResult<AnnualBonusDto>> GetByYear(int year)
    {
        var bonus = await _annualBonusService.GetByYearAsync(year);
        if (bonus == null)
            return NotFound(new { message = $"No hay aguinaldo calculado para {year}" });
        return Ok(bonus);
    }

    /// <summary>Calcula el aguinaldo anual (HU-7.1)</summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<AnnualBonusDto>> Calculate([FromBody] CalculateAnnualBonusDto dto)
    {
        try
        {
            var bonus = await _annualBonusService.CalculateAsync(dto, GetUserId());
            return Ok(bonus);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Aprueba el aguinaldo (HU-7.2)</summary>
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<AnnualBonusDto>> Approve(int id, [FromBody] ApproveAnnualBonusDto dto)
    {
        try
        {
            var bonus = await _annualBonusService.ApproveAsync(id, GetUserId(), dto.Notes);
            return Ok(bonus);
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

    /// <summary>Recalcula el aguinaldo (HU-7.4)</summary>
    [HttpPost("{id}/recalculate")]
    public async Task<ActionResult<AnnualBonusDto>> Recalculate(int id)
    {
        try
        {
            var bonus = await _annualBonusService.RecalculateAsync(id, GetUserId());
            return Ok(bonus);
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
