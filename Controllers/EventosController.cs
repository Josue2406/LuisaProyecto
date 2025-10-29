using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class EventosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔐 Método auxiliar: verifica que el usuario esté logueado
        private bool EsDocenteOAdmin()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Docente" || rol == "Admin";
        }
       

        // 🟢 Vista pública
        public IActionResult Index()
        {
            var eventos = _context.Eventos
                .Where(e => e.Publicado)
                .OrderByDescending(e => e.Fecha)
                .ToList();
            return View(eventos);
        }

        /* 👩‍🏫 CRUD solo si es docente/admin
        public IActionResult Crear()
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index"); // redirige a vista pública

            return View();
        } */
        public IActionResult Crear()
{
    if (!EsDocenteOAdmin())
        return View("~/Views/Shared/AccesoDenegado.cshtml"); // 👈 aquí

    return View();
}

        [HttpPost]
        public IActionResult Crear(Evento model)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            if (!ModelState.IsValid)
                return View(model);

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            model.DocenteId = usuarioId;
            _context.Eventos.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            var evento = _context.Eventos.Find(id);
            if (evento == null) return NotFound();

            return View(evento);
        }

        [HttpPost]
        public IActionResult Editar(Evento model)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            _context.Eventos.Update(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index");

            var evento = _context.Eventos.Find(id);
            if (evento == null) return NotFound();

            _context.Eventos.Remove(evento);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
