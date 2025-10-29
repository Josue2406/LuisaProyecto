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

        public async Task EnviarCorreoAsync(string destino, string asunto, string mensajeHtml)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            string from = emailSettings["From"];
            string displayName = emailSettings["DisplayName"];
            string password = emailSettings["Password"];
            string host = emailSettings["Host"];
            int port = int.Parse(emailSettings["Port"]);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(from, password)
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
    }
}
