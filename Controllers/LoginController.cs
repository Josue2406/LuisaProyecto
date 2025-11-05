using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoLuisa.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();


       [HttpGet("/Recuperar")]
public IActionResult Recuperar()
{
    return View("~/Views/Account/Recuperar.cshtml");
}

[HttpPost("/Recuperar")]
public IActionResult Recuperar(string email)
{
    if (string.IsNullOrEmpty(email))
    {
        ViewBag.Mensaje = "Por favor, ingresa tu correo electrónico.";
        return View("~/Views/Account/Recuperar.cshtml");
    }

    // Simulación de envío de enlace
    ViewBag.Mensaje = $"Se ha enviado un enlace de recuperación al correo: {email}";
    ViewBag.Exito = true;
    return View("~/Views/Account/Recuperar.cshtml");
}



        [HttpPost]
        public IActionResult Index(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Debes ingresar correo y contraseña.";
                return View();
            }

            string hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(contrasena)));
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == correo && u.ContrasenaHash == hash);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View();
            }

            // 🔒 Bloqueo si el usuario fue desactivado manualmente
            if (!usuario.Activo)
            {
                ViewBag.Error = "Tu cuenta está desactivada. Contacta al administrador.";
                return View();
            }

            // 🔒 Bloqueo si su rol fue desactivado por eliminación
            var rolActivo = _context.Usuarios
                .Where(u => u.Rol == usuario.Rol && u.Activo)
                .Any();

            if (!rolActivo && usuario.Rol != "Admin")
            {
                ViewBag.Error = $"Tu rol '{usuario.Rol}' fue desactivado por el administrador.";
                return View();
            }

            // Guardar sesión
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("Rol", usuario.Rol);
            HttpContext.Session.SetString("Nombre", usuario.Nombre);

            // Redirección según el rol
            switch (usuario.Rol)
            {
                case "Admin":
                    return RedirectToAction("Index", "Admin");
                case "Docente":
                    return RedirectToAction("Inicio", "Docente");
                case "Usuario":
                    return RedirectToAction("Inicio", "Usuario");
                default:
                    ViewBag.Error = "Rol desconocido o no autorizado.";
                    return View();
            }
        }

        public IActionResult Logout()
{
    // Eliminar variables de sesión
    HttpContext.Session.Clear();

    // Redirigir al inicio
    return RedirectToAction("Index", "Home");
}

    }
}
