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

        // ✏️ GET: Editar docente
        public IActionResult Editar(int id)
        {
            var docente = _context.Usuarios.Find(id);
            if (docente == null)
                return NotFound();

            return View(docente);
        }
        /*
        // 💾 POST: Editar docente
        [HttpPost]
        public IActionResult Editar(Usuario model)
        {
            var docente = _context.Usuarios.Find(model.Id);
            if (docente == null)
                return NotFound();

            docente.Nombre = model.Nombre;
            docente.Correo = model.Correo;

            _context.SaveChanges();

            TempData["Mensaje"] = $"Datos del docente '{docente.Nombre}' actualizados correctamente.";
            return RedirectToAction("Index");
        } */
        [HttpPost]
        public IActionResult Editar(int id, string nombre, string correo)
        {
            var docente = _context.Usuarios.Find(id);
            if (docente == null)
                return NotFound();

            if (_context.Usuarios.Any(u => u.Correo == correo && u.Id != id))
                return BadRequest("Ya existe otro usuario con ese correo.");

            docente.Nombre = nombre;
            docente.Correo = correo;
            _context.SaveChanges();

            return Ok();
        }


        //Formulario para crear un nuevo docente
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
        /*
        [HttpPost]
public async Task<IActionResult> CrearDocente(string nombre, string correo, string rol = "Docente")
{
    // 🧩 Validar correo existente
    if (_context.Usuarios.Any(u => u.Correo == correo))
    {
        ViewBag.Mensaje = "Ya existe un usuario con este correo.";
        return View();
    }

    // 🧩 Limitar roles válidos (evita Padre, Estudiante, etc.)
    var rolesPermitidos = new[] { "Docente", "Admin" };
    if (!rolesPermitidos.Contains(rol))
    {
        ViewBag.Mensaje = $"El rol '{rol}' no es válido. Solo puedes crear: {string.Join(", ", rolesPermitidos)}.";
        return View();
    }

    // 🧩 Generar contraseña temporal
    string tempPass = Guid.NewGuid().ToString().Substring(0, 8);
    string hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(tempPass)));

    // 🧩 Crear usuario
    var nuevoUsuario = new Usuario
    {
        Nombre = nombre,
        Correo = correo,
        Rol = rol, // <-- ahora puede ser Admin o Docente
        ContrasenaHash = hash,
        Activo = false,
        FechaCreacion = DateTime.Now
    };

    _context.Usuarios.Add(nuevoUsuario);
    await _context.SaveChangesAsync();

    // 🧩 Crear link de activación
    string link = Url.Action("ActivarCuenta", "Account", new { email = correo }, Request.Scheme);

    // 🧩 Cuerpo del correo (diseño HTML elegante)
    string html = $@"
        <h2 style='color:#2c3e50;'>Invitación a la Plataforma Escolar</h2>
        <p>Hola <b>{nombre}</b>, has sido invitado como <b>{rol}</b>.</p>
        <p>Tu contraseña temporal es: <b style='color:#1abc9c;'>{tempPass}</b></p>
        <p>Por favor, haz clic en el siguiente enlace para activar tu cuenta:</p>
        <a href='{link}' style='background:#1abc9c;color:white;padding:10px 15px;text-decoration:none;border-radius:5px;'>Activar mi cuenta</a>
        <p style='margin-top:10px;color:#7f8c8d;font-size:14px;'>Este enlace caduca en 24 horas.</p>";

    await _emailService.EnviarCorreoAsync(correo, $"Invitación como {rol}", html);

    TempData["Mensaje"] = $"Invitación enviada correctamente a {correo} como {rol}.";
    return RedirectToAction("Index");
}
*/

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
