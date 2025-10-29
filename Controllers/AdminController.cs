using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;
using ProyectoLuisa.Services;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoLuisa.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AdminController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Página principal del administrador
        public IActionResult Index()
        {
            var docentes = _context.Usuarios
                .Where(u => u.Rol == "Docente")
                .OrderByDescending(u => u.FechaCreacion)
                .ToList();

            return View(docentes);
        }

        // Formulario para crear un nuevo docente
        public IActionResult CrearDocente() => View();

        [HttpPost]
        public async Task<IActionResult> CrearDocente(string nombre, string correo)
        {
            if (_context.Usuarios.Any(u => u.Correo == correo))
            {
                ViewBag.Mensaje = "Ya existe un usuario con este correo.";
                return View();
            }

            string tempPass = Guid.NewGuid().ToString().Substring(0, 8);
            string hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(tempPass)));

            var docente = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Rol = "Docente",
                ContrasenaHash = hash
            };

            _context.Usuarios.Add(docente);
            await _context.SaveChangesAsync();

            string link = Url.Action("ActivarCuenta", "Account", new { email = correo }, Request.Scheme);
            string html = $@"
                <h2>Invitación a la Plataforma</h2>
                <p>Hola {nombre}, has sido invitado como docente.</p>
                <p>Tu contraseña temporal es: <b>{tempPass}</b></p>
                <p>Haz clic aquí para activar tu cuenta:</p>
                <a href='{link}'>Activar mi cuenta</a>";

            await _emailService.EnviarCorreoAsync(correo, "Invitación Docente", html);

            TempData["Mensaje"] = "Invitación enviada correctamente.";
            return RedirectToAction("Index");
        }

        // Reenviar invitación a un docente existente
        public async Task<IActionResult> ReenviarInvitacion(int id)
        {
            var docente = _context.Usuarios.FirstOrDefault(u => u.Id == id && u.Rol == "Docente");
            if (docente == null) return NotFound();

            string tempPass = Guid.NewGuid().ToString().Substring(0, 8);
            docente.ContrasenaHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(tempPass)));
            await _context.SaveChangesAsync();

            string link = Url.Action("ActivarCuenta", "Account", new { email = docente.Correo }, Request.Scheme);
            string html = $@"
                <h2>Reenvío de Invitación</h2>
                <p>Hola {docente.Nombre}, aquí tienes un nuevo enlace de activación.</p>
                <p>Tu contraseña temporal es: <b>{tempPass}</b></p>
                <p>Activa tu cuenta aquí:</p>
                <a href='{link}'>Activar mi cuenta</a>";

            await _emailService.EnviarCorreoAsync(docente.Correo, "Reenvío de Invitación Docente", html);

            TempData["Mensaje"] = "Invitación reenviada correctamente.";
            return RedirectToAction("Index");
        }
    }
}
