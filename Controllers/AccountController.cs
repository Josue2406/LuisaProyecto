using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;
using ProyectoLuisa.Services;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoLuisa.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ------------------------------------------------------------
        // 🔹 Activar cuenta (cuando el admin invita a un docente)
        // ------------------------------------------------------------
        public IActionResult ActivarCuenta(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ActivarCuenta(string email, string nuevaContrasena)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Correo == email);
            if (user == null)
                return NotFound();

            user.ContrasenaHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(nuevaContrasena))
            );
            user.Activo = true;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Cuenta activada correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Index", "Login");
        }

        // ------------------------------------------------------------
        // 🔹 Recuperar contraseña (formulario donde el usuario escribe su correo)
        // ------------------------------------------------------------
        public IActionResult Recuperar() => View();

        [HttpPost]
        public async Task<IActionResult> Recuperar(string correo)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (user == null)
            {
                ViewBag.Error = "No se encontró una cuenta con ese correo.";
                return View();
            }

            // 🧹 Eliminar tokens expirados antes de crear uno nuevo
            _context.PasswordResetTokens.RemoveRange(
                _context.PasswordResetTokens.Where(t => t.Expira < DateTime.UtcNow)
            );

            // 🔹 Crear nuevo token válido por 1 hora (en UTC)
            string token = Guid.NewGuid().ToString();

            var newToken = new PasswordResetToken
            {
                Correo = correo,
                Token = token,
                Expira = DateTime.UtcNow.AddHours(1) // ✅ UTC
            };

            _context.PasswordResetTokens.Add(newToken);
            await _context.SaveChangesAsync();

            // 🔗 Enlace absoluto para el correo
            string link = Url.Action("Restablecer", "Account", new { token }, Request.Scheme);

            // ✉️ HTML del correo
            string html = $@"
                <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;border:1px solid #ddd;border-radius:8px;'>
                    <h2 style='color:#0d6efd;'>Recuperación de Contraseña</h2>
                    <p>Has solicitado restablecer tu contraseña.</p>
                    <p>Haz clic en el siguiente botón para establecer una nueva:</p>
                    <a href='{link}' style='display:inline-block;margin:10px 0;background:#198754;color:white;padding:10px 20px;border-radius:5px;text-decoration:none;'>
                        Restablecer Contraseña
                    </a>
                    <p style='color:#555;'>Este enlace expirará en 1 hora.</p>
                </div>
            ";

            await _emailService.EnviarCorreoAsync(correo, "Recuperación de Contraseña", html);
            ViewBag.Mensaje = "Se ha enviado un correo con el enlace de recuperación.";
            return View("Index", "Login");
        }

        // ------------------------------------------------------------
        // 🔹 Mostrar formulario de nueva contraseña (GET)
        // ------------------------------------------------------------

        /*
        public IActionResult Restablecer(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Token no válido.";
                return View();
            }

            // 🔍 Validar token aún vigente (comparando con UTC)
            var tokenData = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.Expira > DateTime.UtcNow);

            if (tokenData == null)
            {
                ViewBag.Error = "El enlace de recuperación ha expirado o no es válido.";
                return View();
            }

            ViewBag.Token = token;
            return View();
        } */

public IActionResult Restablecer(string token)
{
    if (string.IsNullOrEmpty(token))
        return View("TokenExpirado");

    var tokenData = _context.PasswordResetTokens
        .FirstOrDefault(t => t.Token == token && t.Expira > DateTime.UtcNow);

    if (tokenData == null)
        return View("TokenExpirado");

    ViewBag.Token = token;
    return View();
}

        // ------------------------------------------------------------
        // 🔹 Guardar nueva contraseña (POST)
        // -------
        // -----------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> Restablecer(string token, string nuevaContrasena)
        {
            var tokenData = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.Expira > DateTime.UtcNow);

            if (tokenData == null)
            {
                ViewBag.Error = "El enlace de recuperación ha expirado o no es válido.";
                return View();
            }

            var user = _context.Usuarios.FirstOrDefault(u => u.Correo == tokenData.Correo);
            if (user == null)
            {
                ViewBag.Error = "Usuario no encontrado.";
                return View();
            }

            // 🔑 Actualizar contraseña
            user.ContrasenaHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(nuevaContrasena))
            );

            // 🧹 Eliminar token usado
            _context.PasswordResetTokens.Remove(tokenData);
            await _context.SaveChangesAsync();

            // ✅ Mostrar página de confirmación
            return View("ConfirmacionCambio");
        }
    }
}
