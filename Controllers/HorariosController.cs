using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class HorariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HorariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool EsDocenteOAdmin()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Docente" || rol == "Admin";
        }

        // 🟢 Vista pública con filtro por sección
        public IActionResult Index(string? seccion)
        {
            var secciones = _context.Horarios
                .Select(h => h.Seccion)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            ViewBag.Secciones = secciones;
            ViewBag.SeccionSeleccionada = seccion;

            var horarios = _context.Horarios
                .Where(h => h.Publicado && (string.IsNullOrEmpty(seccion) || h.Seccion == seccion))
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToList();

            return View(horarios);
        }

        // 👩‍🏫 Crear horario (solo docente o admin)
        public IActionResult Crear()
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            return View();
        }

        [HttpPost]
        public IActionResult Crear(Horario model)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            if (!ModelState.IsValid)
                return View(model);

            model.DocenteId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            _context.Horarios.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 🟡 Editar horario
        public IActionResult Editar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var horario = _context.Horarios.Find(id);
            if (horario == null) return NotFound();

            return View(horario);
        }

        [HttpPost]
        public IActionResult Editar(Horario model)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            if (!ModelState.IsValid)
                return View(model);

            _context.Horarios.Update(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var horario = _context.Horarios.Find(id);
            if (horario == null) return NotFound();

            _context.Horarios.Remove(horario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
