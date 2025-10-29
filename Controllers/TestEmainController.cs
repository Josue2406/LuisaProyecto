/*using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Services;
using System.Threading.Tasks;

namespace ProyectoLuisa.Controllers
{
    public class TestEmailController : Controller
    {
        private readonly EmailService _emailService;

        public TestEmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> Enviar()
        {
            await _emailService.EnviarCorreoAsync(
                "tucorreo@ejemplo.com", 
                "🧪 Prueba de correo - Plataforma Luisa",
                "<h3>¡Hola!</h3><p>Este es un correo de prueba enviado desde tu aplicación ASP.NET Core.</p>"
            );
            return Content("Correo enviado correctamente ✅");
        }
    }
}
*/