namespace SigepApplication.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Envía un correo electrónico. Si falla el envío (credenciales, red, etc.),
    /// el error se registra en el log pero no se lanza excepción, para no
    /// interrumpir el flujo de negocio que lo dispara (aprobar/rechazar, etc.).
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}