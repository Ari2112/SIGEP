//  Esto es una INTERFAZ. Dice QUÉ cosas debe saber hacer el servicio de autenticación,
//  pero no CÓMO las hace. El "cómo" está en AuthService.cs.


using SigepApplication.DTOs.Auth;

namespace SigepApplication.Interfaces;

public interface IAuthService
{
    // Promesa 1: poder validar un usuario y contraseña, y devolver sus datos
    // (o nada, si las credenciales son incorrectas).
    Task<LoginResponseDto?> LoginAsync(string username, string password);

    // Promesa 2: poder devolver los datos del usuario que ya está conectado,
    // a partir de su identificador.
    Task<LoginResponseDto?> GetCurrentUserAsync(int userId);
}
