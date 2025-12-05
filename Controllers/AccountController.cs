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
        public IActionResult ActivarCuenta(string email, string temp)
{
    var user = _context.Usuarios.FirstOrDefault(x => x.Correo == email);
    if (user == null) return NotFound();

    ViewBag.Email = email;
    ViewBag.Temp = temp;
    return View();
}

       [HttpPost]
public async Task<IActionResult> ActivarCuenta(string email, string temporal, string nuevaContrasena)
{
    var user = _context.Usuarios.FirstOrDefault(x => x.Correo == email);

    if (user == null)
    {
        ViewBag.Error = "Usuario no encontrado.";
        return View();
    }

    // Hash de la temporal
    var tempHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(temporal)));

    if (tempHash != user.ContrasenaHash)
    {
        ViewBag.Error = "La contraseña temporal no es válida.";
        return View();
    }

    // Guardar nueva contraseña
    user.ContrasenaHash = Convert.ToBase64String(
        SHA256.HashData(Encoding.UTF8.GetBytes(nuevaContrasena))
    );
    user.Activo = true;

    await _context.SaveChangesAsync();

    TempData["Mensaje"] = "Tu cuenta ha sido activada. Ya puedes iniciar sesión.";

    return RedirectToAction("Index", "Login");
}


        // ------------------------------------------------------------
        // 🔹 Recuperar contraseña (formulario donde el usuario escribe su correo)
        // ------------------------------------------------------------
        [HttpPost]
public async Task<IActionResult> Recuperar(string correo)
{
    var user = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
    if (user == null)
    {
        ViewBag.Error = "No se encontró una cuenta con ese correo.";
        return View();
    }

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

    string link = Url.Action("Restablecer", "Account", new { token }, Request.Scheme);

    string html = $@"
        <h2>Recuperación de contraseña</h2>
        <p>Haz clic en el enlace para restablecerla:</p>
        <a href='{link}'>Restablecer contraseña</a>
    ";

    await _emailService.EnviarCorreoAsync(correo, "Restablecer contraseña", html);

    TempData["Mensaje"] = "Se ha enviado un enlace de recuperación a tu correo.";
    return RedirectToAction("Index", "Login");
}


        // ------------------------------------------------------------
        // 🔹 Mostrar formulario de nueva contraseña (GET)
        // ------------------------------------------------------------

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
        return View("TokenExpirado");

    var user = _context.Usuarios.FirstOrDefault(u => u.Correo == tokenData.Correo);
    if (user == null)
    {
        ViewBag.Error = "Usuario no encontrado.";
        return View();
    }

    user.ContrasenaHash = Convert.ToBase64String(
        SHA256.HashData(Encoding.UTF8.GetBytes(nuevaContrasena))
    );

    _context.PasswordResetTokens.Remove(tokenData);
    await _context.SaveChangesAsync();

    return View("ConfirmacionCambio");
}

    }
}
