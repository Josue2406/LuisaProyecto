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

        // -----------------------------
        // Página principal del admin
        // -----------------------------
        public IActionResult Index()
        {
            var docentes = _context.Usuarios
                .Where(u => u.Rol == "Docente" || u.Rol == "Admin")
                .OrderByDescending(u => u.FechaCreacion)
                .ToList();

            return View(docentes);
        }

        public IActionResult PanelInicio()
        {
            return RedirectToAction("Index", "AdmInicio");
        }

        // -----------------------------
        // Editar docente
        // -----------------------------
        [HttpPost]
        public IActionResult Editar(int id, string nombre, string correo, string rol)
        {
            var docente = _context.Usuarios.Find(id);
            if (docente == null) return NotFound();

            if (_context.Usuarios.Any(u => u.Correo == correo && u.Id != id))
                return BadRequest("Ya existe otro usuario con ese correo.");

            var rolesPermitidos = new[] { "Docente", "Admin" };
            if (!rolesPermitidos.Contains(rol))
                return BadRequest("Rol no permitido.");

            docente.Nombre = nombre;
            docente.Correo = correo;
            docente.Rol = rol;

            _context.SaveChanges();
            return Ok();
        }

        // -----------------------------
        // Eliminar docente
        // -----------------------------
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var docente = _context.Usuarios.Find(id);
            if (docente == null) return NotFound();

            _context.Usuarios.Remove(docente);
            _context.SaveChanges();

            TempData["Mensaje"] = $"Docente '{docente.Nombre}' eliminado correctamente.";
            return RedirectToAction("Index");
        }

        // -----------------------------
        // Crear docente
        // -----------------------------
        public IActionResult CrearDocente() => View();

        [HttpPost]
        public async Task<IActionResult> CrearDocente(string nombre, string correo, string rol = "Docente")
        {
            if (_context.Usuarios.Any(u => u.Correo == correo))
            {
                ViewBag.Mensaje = "Ya existe un usuario con este correo.";
                return View();
            }

            var rolesPermitidos = new[] { "Docente", "Admin" };
            if (!rolesPermitidos.Contains(rol))
            {
                ViewBag.Mensaje = $"Rol no permitido. Solo puedes crear: {string.Join(", ", rolesPermitidos)}.";
                return View();
            }

            var docente = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Rol = rol,
                Activo = false,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(docente);
            await _context.SaveChangesAsync();

            // Crear token y guardarlo en la DB
            string token = Guid.NewGuid().ToString();
            var tokenEntry = new PasswordResetToken
            {
                Correo = correo,
                Token = token,
                Expira = DateTime.UtcNow.AddHours(24) // token válido por 24h
            };
            _context.PasswordResetTokens.Add(tokenEntry);
            await _context.SaveChangesAsync();

            // Generar link con token
            string link = Url.Action("ActivarCuenta", "Account", new { token }, Request.Scheme);

            // Enviar correo de activación
            await _emailService.EnviarCorreoInvitacionDocente(correo, token, link);

            TempData["Mensaje"] = $"Invitación enviada correctamente a {correo} como {rol}.";
            return RedirectToAction("Index");
        }

        // -----------------------------
        // Reenviar invitación a docente existente
        // -----------------------------
        public async Task<IActionResult> ReenviarInvitacion(int id)
        {
            var docente = _context.Usuarios.FirstOrDefault(u => u.Id == id && (u.Rol == "Docente" || u.Rol == "Admin"));
            if (docente == null) return NotFound();

            // Crear nuevo token
            string token = Guid.NewGuid().ToString();
            var tokenEntry = new PasswordResetToken
            {
                Correo = docente.Correo,
                Token = token,
                Expira = DateTime.UtcNow.AddHours(24)
            };
            _context.PasswordResetTokens.Add(tokenEntry);
            await _context.SaveChangesAsync();

            // Generar link con token
            string link = Url.Action("ActivarCuenta", "Account", new { token }, Request.Scheme);

            // Enviar correo
            await _emailService.EnviarCorreoInvitacionDocente(docente.Correo, token, link);

            TempData["Mensaje"] = "Invitación reenviada correctamente.";
            return RedirectToAction("Index");
        }
    }
}
