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
    private readonly IWebHostEnvironment _env;

    public DisabilityController(
        IDisabilityService disabilityService,
        ILogger<DisabilityController> logger,
        IWebHostEnvironment env)
    {
        _disabilityService = disabilityService;
        _logger = logger;
        _env = env;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int? GetEmployeeId()
    {
        var claim = User.FindFirstValue("EmployeeId");
        return claim != null ? int.Parse(claim) : null;
    }

    /// <summary>Obtiene todas las incapacidades con filtros (Admin/RRHH)</summary>
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

    /// <summary>
    /// Sube un documento (imagen/PDF) de incapacidad.
    /// Retorna la ruta relativa para guardar en AttachmentPath.
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<object>> UploadDocument([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No se recibió ningún archivo" });

        // Validar tipo de archivo
        var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Solo se permiten archivos PDF, JPG o PNG" });

        // Validar tamaño (máx 5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "El archivo no puede superar 5MB" });

        try
        {
            var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadDir = Path.Combine(wwwroot, "uploads", "disabilities");
            Directory.CreateDirectory(uploadDir);

            // Nombre único para evitar colisiones
            var ext = Path.GetExtension(file.FileName).ToLower();
            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/disabilities/{uniqueName}";

            return Ok(new
            {
                path = relativePath,
                fileName = file.FileName,
                size = file.Length,
                contentType = file.ContentType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir documento de incapacidad");
            return StatusCode(500, new { message = "Error al guardar el archivo" });
        }
    }

    /// <summary>Obtiene los tipos de incapacidad disponibles</summary>
    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<object>>> GetTypes()
    {
        var types = await _disabilityService.GetTypesAsync();
        return Ok(types);
    }

    /// <summary>Registra una incapacidad (empleado)</summary>
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

    /// <summary>Admin/RRHH puede registrar incapacidad para un empleado específico</summary>
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

    /// <summary>Aprueba o rechaza una incapacidad (Admin/RRHH)</summary>
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
