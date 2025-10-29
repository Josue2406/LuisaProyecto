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

        // 🟢 Vista pública
        public IActionResult Index()
        {
            var horarios = _context.Horarios
                .Where(h => h.Publicado)
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToList();
            return View(horarios);
        }

        // 👩‍🏫 CRUD solo para docentes o admin
        /*public IActionResult Crear()
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            return View();
        } */
          public IActionResult Crear()
{
    if (!EsDocenteOAdmin())
        return View("~/Views/Shared/AccesoDenegado.cshtml"); // 👈 aquí

    return View();
}

        [HttpPost]
        public IActionResult Crear(Horario model)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            model.DocenteId = usuarioId;
            _context.Horarios.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            var horario = _context.Horarios.Find(id);
            if (horario == null) return NotFound();

            return View(horario);
        }

        [HttpPost]
        public IActionResult Editar(Horario model)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            _context.Horarios.Update(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            var horario = _context.Horarios.Find(id);
            if (horario == null) return NotFound();

            _context.Horarios.Remove(horario);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
