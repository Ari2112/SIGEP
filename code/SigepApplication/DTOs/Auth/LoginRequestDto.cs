//  Un "DTO" (Data Transfer Object) es un sobre con datos que viaja
//  entre el frontend y el backend. Este ENTRA:
//  contiene lo que la persona escribió en el formulario de login (usuario y
//  contraseña). No tiene lógica, solo carga la información de un lado a otro.

namespace SigepApplication.DTOs.Auth;

public class LoginRequestDto
{
    // El nombre de usuario que se escribió. Empieza vacío por defecto.
    public string Username { get; set; } = string.Empty;

    // La contraseña que se escribió. También empieza vacía.
    public string Password { get; set; } = string.Empty;
}
