using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace SistemaOnline.Services
{
    // Credenciales y datos del servidor SMTP, leídos desde la sección "Smtp" de
    // configuración (appsettings.json / appsettings.Development.json / variables de
    // entorno) — nunca hardcodeados en el código.
    public class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string From { get; set; } = "";
        public string FromName { get; set; } = "El Rancho Sagrado";
    }

    public interface IEmailService
    {
        Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml);
    }

    // Envío de correo real por SMTP (p.ej. Gmail con una contraseña de aplicación).
    // Reemplaza la simulación anterior: si Smtp no está configurado, falla de forma
    // explícita en vez de fingir que el correo se envió.
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _opciones;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpOptions> opciones, ILogger<SmtpEmailService> logger)
        {
            _opciones = opciones.Value;
            _logger = logger;
        }

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            if (string.IsNullOrWhiteSpace(_opciones.Host) || string.IsNullOrWhiteSpace(_opciones.User) || string.IsNullOrWhiteSpace(_opciones.Password))
            {
                _logger.LogWarning("Smtp no está configurado (appsettings: Smtp:User/Password vacíos); no se envió el correo a {Destinatario}.", destinatario);
                throw new InvalidOperationException("El envío de correo no está configurado: completa la sección \"Smtp\" en appsettings.Development.json.");
            }

            using var cliente = new SmtpClient(_opciones.Host, _opciones.Port)
            {
                EnableSsl = _opciones.EnableSsl,
                Credentials = new NetworkCredential(_opciones.User, _opciones.Password)
            };

            using var mensaje = new MailMessage
            {
                From = new MailAddress(_opciones.From, _opciones.FromName),
                Subject = asunto
            };
            mensaje.To.Add(destinatario);

            // Correo "multipart" (texto plano + HTML) en vez de solo HTML: los filtros de
            // spam califican peor un correo que únicamente trae HTML sin una versión de
            // texto equivalente. El texto plano se deriva del HTML quitando las etiquetas.
            string textoPlano = Regex.Replace(cuerpoHtml, "<[^>]+>", " ");
            textoPlano = Regex.Replace(textoPlano, @"\s+", " ").Trim();
            mensaje.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textoPlano, null, "text/plain"));
            mensaje.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(cuerpoHtml, null, "text/html"));

            await cliente.SendMailAsync(mensaje);
        }
    }
}