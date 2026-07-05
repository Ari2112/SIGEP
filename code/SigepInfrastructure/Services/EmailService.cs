using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SigepApplication.Interfaces;

namespace SigepInfrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Envío de correo deshabilitado (EmailSettings:Enabled=false). Se omitió el correo a {ToEmail} con asunto '{Subject}'.", toEmail, subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("No se pudo enviar el correo '{Subject}': el destinatario no tiene una dirección de correo registrada.", subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogWarning("No se pudo enviar el correo '{Subject}' a {ToEmail}: faltan credenciales SMTP en la configuración (EmailSettings).", subject, toEmail);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Correo enviado a {ToEmail} con asunto '{Subject}'.", toEmail, subject);
        }
        catch (Exception ex)
        {
            // No relanzamos la excepción: el correo es un efecto secundario y no debe
            // interrumpir la operación de negocio (aprobar/rechazar/generar planilla, etc.)
            _logger.LogError(ex, "Error al enviar el correo a {ToEmail} con asunto '{Subject}'.", toEmail, subject);
        }
    }
}