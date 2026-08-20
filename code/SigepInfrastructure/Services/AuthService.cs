// Se encarga de TODO lo relacionado con iniciar sesión: revisa que el usuario 
//y la contraseña sean correctos,
//  y si todo está bien, le entrega al usuario un token
//  para que pueda moverse por el sistema sin tener que volver a escribir su
//  clave en cada pantalla.
// ============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SigepApplication.DTOs.Auth;
using SigepApplication.Interfaces;
using SigepInfrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SigepInfrastructure.Services;

public class AuthService : IAuthService
{
    //  - _context: es la conexión a la base de datos (para buscar usuarios).
    //  - _configuration: nos deja leer valores de configuración, como la
    //    llave secreta con la que se firman los tokens.
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    //  LoginAsync: este es el método que se ejecuta cuando alguien intenta
    //  entrar al sistema con su usuario y contraseña.
    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        // Buscamos en la base de datos un usuario que tenga ese nombre y que
        // además esté activo. De paso traemos su información de empleado y su
        // rol para no hacer más consultas luego.
        var user = await _context.Users
            .Include(u => u.Employee)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        // Si no encontramos a nadie con ese usuario, devolvemos "nada"
        // (null), que el sistema interpreta como "credenciales incorrectas".
        if (user == null)
            return null;

        // Aquí comparamos la contraseña que escribió la persona contra la
        // versión encriptada guardada en la base
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        // Si el usuario no tiene rol asignado, por seguridad lo tratamos
        // como "Empleado" (el rol con menos permisos).
        string roleName = user.Role != null ? user.Role.Name : "Empleado";

        // Generamos el "pase de entrada" (token) con los datos del usuario.
        var token = GenerateJwtToken(user.Id, user.Username, roleName, user.EmployeeId);

        // Devolvemos al frontend un paquete con el token y los datos básicos
        // que la pantalla necesita mostrar (nombre, rol, etc.).
        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = roleName,
            UserId = user.Id,
            EmployeeId = user.EmployeeId,
            FullName = user.Employee?.FullName
        };
    }

    //  GetCurrentUserAsync: sirve para volver a obtener los datos del usuario
    //  que YA inició sesión (por ejemplo, al refrescar la página). Aquí no
    //  pedimos contraseña porque la persona ya está autenticada con su token.
    public async Task<LoginResponseDto?> GetCurrentUserAsync(int userId)
    {
        // Buscamos al usuario por su identificador, siempre que siga activo.
        var user = await _context.Users
            .Include(u => u.Employee)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
            return null;

        string roleName = user.Role != null ? user.Role.Name : "Empleado";

        // Devolvemos sus datos. El token va vacío a propósito, porque en este
        // caso solo queremos refrescar la información, no generar un pase nuevo.
        return new LoginResponseDto
        {
            Token = string.Empty,
            Username = user.Username,
            Role = roleName,
            UserId = user.Id,
            EmployeeId = user.EmployeeId,
            FullName = user.Employee?.FullName
        };
    }
    //  ChangePasswordAsync: deja que un usuario YA conectado cambie su
    //  propia contraseña. Primero verificamos que la contraseña actual
    //  que escribió sea correcta (igual que en el login); si no lo es,
    //  no se cambia nada. Si es correcta, guardamos la nueva contraseña
    //  encriptada con BCrypt, igual que se hace con todas las contraseñas
    //  del sistema.
    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "La nueva contraseña debe tener al menos 6 caracteres.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        if (user == null)
            return (false, "Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return (false, "La contraseña actual no es correcta.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, null);
    }

    //  GenerateJwtToken: arma el "pase de entrada" (el token JWT).
    private string GenerateJwtToken(int userId, string username, string role, int? employeeId = null)
    {
        // Leemos la configuración del token (llave secreta, emisor, etc.).
        // Si por alguna razón no está configurada, usamos un valor por defecto.
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "MySecretKeyForSigepSystem2026VeryLongAndSecure123!";

        // Con esa llave secreta preparamos la "firma" del carnet. Es lo que
        // garantiza que el token salió de nuestro servidor y no fue alterado.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Los "claims" son los datos que viajan dentro del carnet: quién es la
        // persona, su nombre de usuario, un identificador único, etc.
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Agregamos el o los roles. Para el administrador se escriben AMBAS
        // variantes ("Admin" y "Administrador") para que las pantallas funcionen
        // sin importar cuál nombre usó cada parte del sistema.
        foreach (var roleClaim in BuildRoleClaims(role))
            claims.Add(roleClaim);

        // Si el usuario está ligado a un empleado, también guardamos ese dato
        // dentro del token para tenerlo a mano.
        if (employeeId.HasValue)
            claims.Add(new Claim("EmployeeId", employeeId.Value.ToString()));

        // Finalmente armamos el token con todos esos datos. Le ponemos una
        // fecha de vencimiento de 8 horas: pasado ese tiempo, la persona debe
        // volver a iniciar sesión (es una medida de seguridad estándar).
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "SigepAPI",
            audience: jwtSettings["Audience"] ?? "SigepClient",
            claims: claims.ToArray(),
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        // Convertimos el token a texto para enviarlo al
        // frontend, que lo guardará y lo usará en cada petición.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    //  BuildRoleClaims: resuelve un problema histórico del proyecto. En la base
    //  de datos el rol se guardó como "Administrador", pero algunas pantallas
    //  del backend se programaron esperando "Admin". Eso causaba errores de
    //  permisos (error 403). La solución: cuando alguien es administrador, le
    //  agregamos ambas palabras en el token, así cualquier pantalla lo
    //  reconoce sin importar cuál nombre haya usado.
    private static IEnumerable<Claim> BuildRoleClaims(string role)
    {
        // Usamos un conjunto que ignora mayúsculas/minúsculas para no repetir
        // roles por diferencias de escritura.
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { role };

        // Detectamos si la persona es administrador (con cualquiera de los
        // dos nombres posibles).
        bool esAdministrador =
            role.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        // Si lo es, agregamos las dos variantes para cubrir todos los casos.
        if (esAdministrador)
        {
            roles.Add("Administrador");
            roles.Add("Admin");
        }

        // Convertimos cada nombre de rol en un "claim" de rol para el token.
        return roles.Select(r => new Claim(ClaimTypes.Role, r));
    }
}