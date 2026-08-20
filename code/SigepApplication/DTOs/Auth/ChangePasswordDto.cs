namespace SigepApplication.DTOs.Auth;

// Lo que envía el formulario de "Cambiar contraseña" en Mi Perfil:
// la contraseña actual (para confirmar que es realmente la persona
// dueña de la cuenta) y la contraseña nueva que quiere usar.
public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}