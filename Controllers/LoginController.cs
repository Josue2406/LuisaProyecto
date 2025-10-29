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

            if (!usuario.Activo && usuario.Rol != "Admin")
            {
                ViewBag.Error = "Tu cuenta aún no ha sido activada. Revisa tu correo.";
                return View();
            }

            // ✅ GUARDAR SESIÓN DESPUÉS DE VALIDAR
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
        // 🔹 Cerrar sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
