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

        // ----------- MÉTODO PRINCIPAL -------------
        public async Task EnviarCorreoAsync(string destino, string asunto, string mensajeHtml, string provider = null)
        {
            // Obtiene el proveedor por defecto si no se indica
            provider ??= _config["EmailProviders:DefaultProvider"];

            var settings = provider switch
            {
                "Gmail" => _config.GetSection("EmailSettingsGmail"),
                "Mailtrap" => _config.GetSection("EmailSettingsMailtrap"),
                _ => throw new Exception("Proveedor de correo no válido.")
            };

            string from = settings["From"];
            string displayName = settings["DisplayName"];
            string password = settings["Password"];
            string host = settings["Host"];
            int port = int.Parse(settings["Port"]);
            string username = settings["Username"];

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = provider == "Gmail",  // Gmail usa SSL, Mailtrap no obligatorio
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

            try
            {
                await client.SendMailAsync(mail);
                Console.WriteLine($"[OK] Correo enviado a {destino} usando {provider}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("[INNER] " + ex.InnerException.Message);
                throw;
            }
        }

        // ----------- MÉTODO PERSONALIZADO -------------
        public async Task EnviarCorreoInvitacionDocente(string correo, string contrasenaTemporal)
        {
            string link = $"https://tusitio.com/Account/ActivarCuenta?email={correo}&temp={contrasenaTemporal}";

            string html = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #ddd;border-radius:8px;'>
                <h2 style='color:#0d6efd;'>¡Bienvenido a la plataforma!</h2>
                <p>Tu contraseña temporal es:</p>
                <p style='font-size:18px;font-weight:bold;'>{contrasenaTemporal}</p>
                <a href='{link}' style='display:inline-block;margin:10px 0;background:#198754;color:white;padding:10px 20px;border-radius:5px;text-decoration:none;'>
                    Activar Cuenta
                </a>
            </div>
            ";

            // Enviar usando el proveedor por defecto (Mailtrap)
            await EnviarCorreoAsync(correo, "Activación de Cuenta Docente", html);
        }
    }
}
