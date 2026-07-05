//  Este controlador atiende todo lo relacionado con el inicio de
//  sesión. Solo recibe la solicitud, se la pasa
//  al servicio (AuthService) y devuelve la respuesta.

using Microsoft.AspNetCore.Mvc;
using SigepApplication.DTOs.Auth;
using SigepApplication.Interfaces;

namespace SigepAPI.Controllers;

// [ApiController] le dice a .NET que esta clase atiende peticiones web.
// [Route] define la "dirección" base: api/v1/auth
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    // Herramientas que recibe el controlador:
    //  - _authService: el que realmente valida usuarios y genera tokens.
    //  - _logger: una "libreta de notas" para registrar errores y poder
    //    revisarlos después si algo falla.
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    //  POST api/v1/auth/login
    //  Se ejecuta cuando alguien envía su usuario y contraseña desde el login.
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            // Le pedimos al servicio que valide las credenciales.
            var result = await _authService.LoginAsync(request.Username, request.Password);

            // Si el servicio devuelve "nada", es que el usuario o la clave
            // estaban mal. Respondemos con un 401 (No autorizado).
            if (result == null)
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            // Si todo salió bien, devolvemos el token y los datos del usuario.
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Si ocurre un error inesperado
            // respondemos con un error genérico (500) para no exponer detalles.
            _logger.LogError(ex, "Error en login");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
    //  GET api/v1/auth/me
    //  Devuelve los datos del usuario que YA inició sesión. Se usa, por
    //  ejemplo, cuando se refresca la página y queremos saber "¿quién es la
    //  persona que está conectada ahora mismo?".
    [HttpGet("me")]
    public async Task<ActionResult<LoginResponseDto>> GetCurrentUser()
    {
        try
        {
            // Sacamos el identificador del usuario desde el token que viene
            // en la petición (ese dato lo guardamos cuando se generó el token).
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Si no viene un identificador válido, el token está malo o vencido.
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            // Buscamos los datos actuales de ese usuario.
            var result = await _authService.GetCurrentUserAsync(userId);

            // Si ya no existe (por ejemplo, lo desactivaron), avisamos.
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
