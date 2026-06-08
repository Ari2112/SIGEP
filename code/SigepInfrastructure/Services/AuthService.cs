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
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        string roleName = user.Role != null ? user.Role.Name : "Empleado";

        var token = GenerateJwtToken(user.Id, user.Username, roleName, user.EmployeeId);

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

    public async Task<LoginResponseDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
            return null;

        string roleName = user.Role != null ? user.Role.Name : "Empleado";

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

    private string GenerateJwtToken(int userId, string username, string role, int? employeeId = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "MySecretKeyForSigepSystem2026VeryLongAndSecure123!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Claims de rol. Para el administrador se emiten AMBAS variantes
        // ("Admin" y "Administrador") de modo que cualquier [Authorize(Roles = ...)]
        // funcione sin importar la grafia usada en cada controlador.
        foreach (var roleClaim in BuildRoleClaims(role))
            claims.Add(roleClaim);

        if (employeeId.HasValue)
            claims.Add(new Claim("EmployeeId", employeeId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "SigepAPI",
            audience: jwtSettings["Audience"] ?? "SigepClient",
            claims: claims.ToArray(),
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Construye los claims de rol para el usuario. El rol del administrador
    /// aparece en la base de datos como "Administrador", pero algunos controladores
    /// historicamente usaron "Admin". Para garantizar consistencia y no romper
    /// ninguna autorizacion existente, el administrador recibe ambas variantes.
    /// </summary>
    private static IEnumerable<Claim> BuildRoleClaims(string role)
    {
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { role };

        bool esAdministrador =
            role.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        if (esAdministrador)
        {
            roles.Add("Administrador");
            roles.Add("Admin");
        }

        return roles.Select(r => new Claim(ClaimTypes.Role, r));
    }
}