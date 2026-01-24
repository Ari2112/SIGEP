using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Auth;
using SigepApplication.Interfaces;

namespace SigepAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request.Username, request.Password);

            if (result == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<LoginResponseDto>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var result = await _authService.GetCurrentUserAsync(userId);

            if (result == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo usuario actual");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
