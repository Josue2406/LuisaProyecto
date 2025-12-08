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

        // GET: Activar cuenta con token
        public IActionResult ActivarCuenta(string token)
        {
            if (string.IsNullOrEmpty(token))
                return View("TokenExpirado");

            var tokenData = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.Expira > DateTime.UtcNow);

            if (tokenData == null)
                return View("TokenExpirado");

            ViewBag.Token = token;
            return View(); // ActivarCuenta.cshtml
        }

        // POST: Activar cuenta y establecer contraseña
        [HttpPost] 
        public async Task<IActionResult> ActivarCuenta(string token, string nuevaContrasena)
        {
            if (string.IsNullOrEmpty(token))
                return View("TokenExpirado");

            var tokenData = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.Expira > DateTime.UtcNow);

            if (tokenData == null)
                return View("TokenExpirado");

            var user = _context.Usuarios.FirstOrDefault(u => u.Correo == tokenData.Correo);
            if (user == null) return View("TokenExpirado");

            user.ContrasenaHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(nuevaContrasena))
            );

            user.Activo = true;

            _context.PasswordResetTokens.Remove(tokenData);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Cuenta activada correctamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Index", "Login");
        }

        // GET: Solicitar recuperación de contraseña
        [HttpGet]
public IActionResult Recuperar()
{
    return View(); // Vista con el formulario para escribir el correo
}

[HttpPost]
public async Task<IActionResult> Recuperar(string correo)
{
    var user = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
    if (user == null)
    {
        ViewBag.Error = "No se encontró una cuenta con ese correo.";
        return View();
    }

    // Limpiar tokens expirados
    _context.PasswordResetTokens.RemoveRange(
        _context.PasswordResetTokens.Where(t => t.Expira < DateTime.UtcNow)
    );

    string token = Guid.NewGuid().ToString();
    var newToken = new PasswordResetToken
    {
        Correo = correo,
        Token = token,
        Expira = DateTime.UtcNow.AddHours(1)
    };

    _context.PasswordResetTokens.Add(newToken);
    await _context.SaveChangesAsync();

    // Generar link de recuperación
    string link = Url.Action("Restablecer", "Account", new { token }, Request.Scheme);

    // Enviar correo
    await _emailService.EnviarCorreoRecuperacion(correo, link);

    TempData["Mensaje"] = "Se ha enviado un enlace de recuperación a tu correo.";
    return RedirectToAction("Index", "Login");
}



        // GET: Restablecer contraseña
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

        // POST: Guardar nueva contraseña
        [HttpPost]
        public async Task<IActionResult> Restablecer(string token, string nuevaContrasena)
        {
            var tokenData = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.Expira > DateTime.UtcNow);

            if (tokenData == null)
                return View("TokenExpirado");

            var user = _context.Usuarios.FirstOrDefault(u => u.Correo == tokenData.Correo);
            if (user == null) return View("TokenExpirado");

            user.ContrasenaHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(nuevaContrasena)));

            _context.PasswordResetTokens.Remove(tokenData);
            await _context.SaveChangesAsync();

            return View("ConfirmacionCambio");
        }
    }
}
