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

        // GET: /Horarios
        public IActionResult Index(string? dia, string? grupo, string? profesor)
        {
            var horarios = _context.Horarios.AsQueryable();

            if (!string.IsNullOrEmpty(dia))
                horarios = horarios.Where(h => h.DiaSemana.Contains(dia));

            if (!string.IsNullOrEmpty(grupo))
                horarios = horarios.Where(h => h.Grupo.Contains(grupo));

            if (!string.IsNullOrEmpty(profesor))
                horarios = horarios.Where(h => h.Profesor.Contains(profesor));

            ViewBag.EsDocenteOAdmin = EsDocenteOAdmin();
            return View(horarios.ToList());
        }

        // GET: /Horarios/Crear
        public IActionResult Crear()
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            return View();
        }

        // POST: /Horarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Horario model)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            if (!ModelState.IsValid)
                return View(model);

            _context.Horarios.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /Horarios/Editar/5
        public IActionResult Editar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var horario = _context.Horarios.Find(id);
            if (horario == null)
                return NotFound();

            return View(horario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            if (horario == null)
                return NotFound();

            _context.Horarios.Remove(horario);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
