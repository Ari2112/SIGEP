using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Permissions;
using SigepApplication.Interfaces;
using System.Security.Claims;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int? GetEmployeeId()
    {
        var claim = User.FindFirstValue("EmployeeId");
        return claim != null ? int.Parse(claim) : null;
    }

    // === TIPOS DE PERMISOS ===

    /// <summary>
    /// Obtiene todos los tipos de permisos activos
    /// </summary>
    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<PermissionTypeDto>>> GetPermissionTypes()
    {
        var types = await _permissionService.GetPermissionTypesAsync();
        return Ok(types);
    }

    /// <summary>
    /// Obtiene un tipo de permiso por ID
    /// </summary>
    [HttpGet("types/{id}")]
    public async Task<ActionResult<PermissionTypeDto>> GetPermissionType(int id)
    {
        var type = await _permissionService.GetPermissionTypeByIdAsync(id);
        if (type == null)
            return NotFound(new { message = "Tipo de permiso no encontrado" });
        return Ok(type);
    }

    // === SOLICITUDES ===

    /// <summary>
    /// Obtiene las solicitudes de permisos del empleado actual
    /// </summary>
    [HttpGet("requests/my")]
    public async Task<ActionResult<IEnumerable<PermissionRequestDto>>> GetMyRequests()
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var requests = await _permissionService.GetEmployeeRequestsAsync(employeeId.Value);
        return Ok(requests);
    }

    /// <summary>
    /// Obtiene una solicitud por su ID
    /// </summary>
    [HttpGet("requests/{id}")]
    public async Task<ActionResult<PermissionRequestDto>> GetRequest(int id)
    {
        var request = await _permissionService.GetRequestByIdAsync(id);
        
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
    public async Task<ActionResult<IEnumerable<PermissionRequestDto>>> GetPendingRequests()
    {
        var requests = await _permissionService.GetPendingRequestsAsync();
        return Ok(requests);
    }

    /// <summary>
    /// Obtiene todas las solicitudes con filtros opcionales (Admin/RRHH)
    /// </summary>
    [HttpGet("requests")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<IEnumerable<PermissionRequestDto>>> GetAllRequests([FromQuery] PermissionRequestFilterDto? filter)
    {
        var requests = await _permissionService.GetAllRequestsAsync(filter);
        return Ok(requests);
    }

    /// <summary>
    /// Crea una nueva solicitud de permiso
    /// </summary>
    [HttpPost("requests")]
    public async Task<ActionResult<PermissionRequestDto>> CreateRequest([FromBody] CreatePermissionRequestDto dto)
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

            var request = await _permissionService.CreateRequestAsync(dto, GetUserId());
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
    /// Aprueba una solicitud de permiso (Admin/RRHH/Jefatura)
    /// </summary>
    [HttpPost("requests/{id}/approve")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<PermissionRequestDto>> ApproveRequest(int id, [FromBody] ApprovalDto? dto)
    {
        try
        {
            var request = await _permissionService.ApproveRequestAsync(id, GetUserId(), dto?.Comments);
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
    /// Rechaza una solicitud de permiso (Admin/RRHH/Jefatura)
    /// </summary>
    [HttpPost("requests/{id}/reject")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<PermissionRequestDto>> RejectRequest(int id, [FromBody] RejectionDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { message = "Debe proporcionar un motivo de rechazo" });

            var request = await _permissionService.RejectRequestAsync(id, GetUserId(), dto.Reason);
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
    /// Cancela una solicitud de permiso
    /// </summary>
    [HttpPost("requests/{id}/cancel")]
    public async Task<ActionResult<PermissionRequestDto>> CancelRequest(int id, [FromBody] CancellationDto? dto)
    {
        try
        {
            var existingRequest = await _permissionService.GetRequestByIdAsync(id);
            if (existingRequest == null)
                return NotFound(new { message = "Solicitud no encontrada" });

            // Verificar permisos
            var employeeId = GetEmployeeId();
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            if (existingRequest.EmployeeId != employeeId && role != "Admin" && role != "RRHH")
                return Forbid();

            var request = await _permissionService.CancelRequestAsync(id, GetUserId(), dto?.Reason);
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
    /// Obtiene el resumen de uso de permisos del empleado actual
    /// </summary>
    [HttpGet("usage")]
    public async Task<ActionResult<PermissionUsageSummaryDto>> GetMyUsageSummary([FromQuery] int? year)
    {
        var employeeId = GetEmployeeId();
        if (!employeeId.HasValue)
            return BadRequest(new { message = "Usuario no tiene empleado asociado" });

        var targetYear = year ?? DateTime.Now.Year;
        var summary = await _permissionService.GetUsageSummaryAsync(employeeId.Value, targetYear);
        return Ok(summary);
    }

    /// <summary>
    /// Obtiene el resumen de uso de permisos de un empleado específico (Admin/RRHH)
    /// </summary>
    [HttpGet("usage/{employeeId}")]
    [Authorize(Roles = "Admin,RRHH,Jefatura")]
    public async Task<ActionResult<PermissionUsageSummaryDto>> GetEmployeeUsageSummary(int employeeId, [FromQuery] int? year)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var summary = await _permissionService.GetUsageSummaryAsync(employeeId, targetYear);
        return Ok(summary);
    }
}
