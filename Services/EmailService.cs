using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ProyectoLuisa.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // Método genérico para enviar correo
        public async Task EnviarCorreoAsync(string destino, string asunto, string mensajeHtml)
        {
            var settings = _config.GetSection("EmailSettingsGmail");

            string from = settings["From"];
            string displayName = settings["DisplayName"];
            string username = settings["Username"];
            string password = settings["Password"];
            string host = settings["Host"];
            int port = int.Parse(settings["Port"]);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(from, displayName),
                Subject = asunto,
                Body = mensajeHtml,
                IsBodyHtml = true
            };
            mail.To.Add(destino);

            await client.SendMailAsync(mail);
        }

        // Enviar correo de activación con token
        public async Task EnviarCorreoInvitacionDocente(string correo, string token, string link)
        {
            string html = $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #ddd;border-radius:8px;'>
                    <h2 style='color:#0d6efd;'>¡Bienvenido a la plataforma!</h2>
                    <p>Has sido invitado como docente.</p>
                    <p>Haz clic en el siguiente enlace para activar tu cuenta:</p>
                    <a href='{link}' style='display:inline-block;margin:10px 0;background:#198754;color:white;padding:10px 20px;border-radius:5px;text-decoration:none;'>
                        Activar Cuenta
                    </a>
                    <p>Este enlace expira en 24 horas.</p>
                </div>";

            await EnviarCorreoAsync(correo, "Activación de Cuenta Docente", html);
        }

        // Enviar correo de recuperación de contraseña
        public async Task EnviarCorreoRecuperacion(string correo, string linkRestablecer)
{
    string html = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #ddd;border-radius:8px;'>
            <h2 style='color:#0d6efd;'>Recuperación de contraseña</h2>
            <p>Haz clic en el enlace para restablecer tu contraseña:</p>
            <a href='{linkRestablecer}' style='display:inline-block;margin:10px 0;background:#198754;color:white;padding:10px 20px;border-radius:5px;text-decoration:none;'>
                Restablecer contraseña
            </a>
            <p>Este enlace expira en 1 hora.</p>
        </div>";

    await EnviarCorreoAsync(correo, "Restablecer contraseña", html);
}


    }

    
}
