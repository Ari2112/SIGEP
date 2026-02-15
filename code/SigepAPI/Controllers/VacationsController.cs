using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Vacations;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VacationsController : ControllerBase
{
    private readonly IVacationService _vacationService;

    public VacationsController(IVacationService vacationService)
    {
        _vacationService = vacationService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int? GetEmployeeId()
    {
        var claim = User.FindFirstValue("EmployeeId");
        return claim != null ? int.Parse(claim) : null;
    }

    // === SALDO DE VACACIONES ===

    /// <summary>
    /// Obtiene el saldo de vacaciones del empleado actual
    /// </summary>
    [HttpGet("balance")]
    public async Task<ActionResult<VacationBalanceDto>> GetMyBalance([FromQuery] int? year)
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var targetYear = year ?? DateTime.Now.Year;
        var balance = await _vacationService.GetBalanceAsync(employeeId.Value, targetYear);
        
        if (balance == null)
        {
            // Inicializar saldo si no existe
            balance = await _vacationService.InitializeBalanceAsync(employeeId.Value, targetYear);
        }
        
        return Ok(balance);
    }

    /// <summary>
    /// Obtiene el saldo de vacaciones de un empleado específico (Admin/RRHH)
    /// </summary>
    [HttpGet("balance/{employeeId}")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<VacationBalanceDto>> GetEmployeeBalance(int employeeId, [FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var balance = await _vacationService.GetBalanceAsync(employeeId, targetYear);
        
        if (balance == null)
            return NotFound(new { message = "No se encontró saldo de vacaciones para este año" });
        
        return Ok(balance);
    }

    /// <summary>
    /// Obtiene el historial de saldos del empleado actual
    /// </summary>
    [HttpGet("balance/history")]
    public async Task<ActionResult<IEnumerable<VacationBalanceDto>>> GetMyBalanceHistory()
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var history = await _vacationService.GetBalanceHistoryAsync(employeeId.Value);
        return Ok(history);
    }

    // === SOLICITUDES ===

    /// <summary>
    /// Obtiene las solicitudes de vacaciones del empleado actual
    /// </summary>
    [HttpGet("requests/my")]
    public async Task<ActionResult<IEnumerable<VacationRequestDto>>> GetMyRequests()
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var requests = await _vacationService.GetEmployeeRequestsAsync(employeeId.Value);
        return Ok(requests);
    }

    /// <summary>
    /// Obtiene una solicitud por su ID
    /// </summary>
    [HttpGet("requests/{id}")]
    public async Task<ActionResult<VacationRequestDto>> GetRequest(int id)
    {
        var request = await _vacationService.GetRequestByIdAsync(id);
        
        if (request == null)
            return NotFound(new { message = "Solicitud no encontrada" });

        // Verificar permisos
        var employeeId = GetEmployeeId();
        var role = User.FindFirstValue(ClaimTypes.Role);
        
        if (role != "Admin" && role != "RRHH" && role != "Jefatura" && request.EmployeeId != employeeId)
            return Forbid();

        return Ok(request);
    }

    /// <summary>
    /// Obtiene todas las solicitudes pendientes (Admin/RRHH/Jefatura)
    /// </summary>
    [HttpGet("requests/pending")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<IEnumerable<VacationRequestDto>>> GetPendingRequests()
    {
        var requests = await _vacationService.GetPendingRequestsAsync();
        return Ok(requests);
    }

    /// <summary>
    /// Obtiene todas las solicitudes con filtros opcionales (Admin/RRHH)
    /// </summary>
    [HttpGet("requests")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<IEnumerable<VacationRequestDto>>> GetAllRequests([FromQuery] VacationRequestFilterDto? filter)
    {
        var requests = await _vacationService.GetAllRequestsAsync(filter);
        return Ok(requests);
    }

    /// <summary>
    /// Crea una nueva solicitud de vacaciones
    /// </summary>
    [HttpPost("requests")]
    public async Task<ActionResult<VacationRequestDto>> CreateRequest([FromBody] CreateVacationRequestDto dto)
    {
        try
        {
            var employeeId = GetEmployeeId();
            
            // Si no se especifica empleado, usar el del usuario actual
            if (dto.EmployeeId == 0)
            {
                if (!employeeId.HasValue)
                    return BadRequest(new { message = "Usuario no tiene empleado asociado" });
                dto.EmployeeId = employeeId.Value;
            }
            else
            {
                // Verificar permisos si es para otro empleado
                var role = User.FindFirstValue(ClaimTypes.Role);
                if (dto.EmployeeId != employeeId && role != "Admin" && role != "RRHH")
                    return Forbid();
            }

            var request = await _vacationService.CreateRequestAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza una solicitud pendiente
    /// </summary>
    [HttpPut("requests/{id}")]
    public async Task<ActionResult<VacationRequestDto>> UpdateRequest(int id, [FromBody] UpdateVacationRequestDto dto)
    {
        try
        {
            var existingRequest = await _vacationService.GetRequestByIdAsync(id);
            if (existingRequest == null)
                return NotFound(new { message = "Solicitud no encontrada" });

            // Verificar permisos
            var employeeId = GetEmployeeId();
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            if (existingRequest.EmployeeId != employeeId && role != "Admin" && role != "RRHH")
                return Forbid();

            var request = await _vacationService.UpdateRequestAsync(id, dto, GetUserId());
            return Ok(request);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Aprueba una solicitud de vacaciones (Admin/RRHH/Jefatura)
    /// </summary>
    [HttpPost("requests/{id}/approve")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<VacationRequestDto>> ApproveRequest(int id, [FromBody] ApprovalDto? dto)
    {
        try
        {
            var request = await _vacationService.ApproveRequestAsync(id, GetUserId(), dto?.Comments);
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

    /// <summary>
    /// Rechaza una solicitud de vacaciones (Admin/RRHH/Jefatura)
    /// </summary>
    [HttpPost("requests/{id}/reject")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<VacationRequestDto>> RejectRequest(int id, [FromBody] RejectionDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Debe proporcionar un motivo de rechazo" });

            var request = await _vacationService.RejectRequestAsync(id, GetUserId(), dto.Reason);
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

    /// <summary>
    /// Cancela una solicitud de vacaciones
    /// </summary>
    [HttpPost("requests/{id}/cancel")]
    public async Task<ActionResult<VacationRequestDto>> CancelRequest(int id, [FromBody] CancellationDto? dto)
    {
        try
        {
            var existingRequest = await _vacationService.GetRequestByIdAsync(id);
            if (existingRequest == null)
                return NotFound(new { message = "Solicitud no encontrada" });

            // Verificar permisos
            var employeeId = GetEmployeeId();
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            if (existingRequest.EmployeeId != employeeId && role != "Admin" && role != "RRHH")
                return Forbid();

            var request = await _vacationService.CancelRequestAsync(id, GetUserId(), dto?.Reason);
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

    /// <summary>
    /// Obtiene el historial de cambios de una solicitud
    /// </summary>
    [HttpGet("requests/{id}/history")]
    public async Task<ActionResult<IEnumerable<VacationRequestHistoryDto>>> GetRequestHistory(int id)
    {
        var existingRequest = await _vacationService.GetRequestByIdAsync(id);
        if (existingRequest == null)
            return NotFound(new { message = "Solicitud no encontrada" });

        // Verificar permisos
        var employeeId = GetEmployeeId();
        var role = User.FindFirstValue(ClaimTypes.Role);
        
        if (existingRequest.EmployeeId != employeeId && role != "Admin" && role != "RRHH" && role != "Jefatura")
            return Forbid();

        var history = await _vacationService.GetRequestHistoryAsync(id);
        return Ok(history);
    }
}

// DTOs auxiliares para las acciones
public class ApprovalDto
{
    public string? Comments { get; set; }
}

public class RejectionDto
{
    public string Reason { get; set; } = string.Empty;
}

public class CancellationDto
{
    public string? Reason { get; set; }
}
