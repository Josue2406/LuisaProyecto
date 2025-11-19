using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Noticias()
    {
        return View();
    }

        // 📌 PÁGINA PRINCIPAL: /Public
        public IActionResult Index()
        {
            var eventos = _context.Eventos
                .Where(e => e.Publicado)
                .OrderByDescending(e => e.Fecha)
                .Take(3)
                .ToList();

            return View("~/Views/Public/Inicio/Index.cshtml", eventos);
        }

        // 📅 HORARIOS PÚBLICOS: /Public/Horarios
        public IActionResult Horarios(string? seccionSeleccionada, bool verTodos = false)
        {
            // 🔹 Obtener todas las secciones existentes
            var secciones = _context.Horarios
                .Where(h => !string.IsNullOrEmpty(h.Seccion))
                .Select(h => h.Seccion.Trim())
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            ViewBag.Secciones = secciones;
            ViewBag.SeccionSeleccionada = seccionSeleccionada;
            ViewBag.VerTodos = verTodos;

            // 🔹 Diccionario: DocenteId → Nombre
            var docentesDict = _context.Usuarios
                .ToDictionary(u => u.Id, u => u.Nombre);

            ViewBag.Docentes = docentesDict;

            // 🔹 Filtrar horarios
            var horarios = _context.Horarios.AsQueryable();

            if (!string.IsNullOrEmpty(seccionSeleccionada))
                horarios = horarios.Where(h => h.Seccion == seccionSeleccionada);

            // 🔹 Agrupar por sección
            var horariosAgrupados = horarios
                .AsEnumerable()
                .GroupBy(h => string.IsNullOrWhiteSpace(h.Seccion) ? "Sin sección" : h.Seccion.Trim())
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(h => ObtenerOrdenDia(h.DiaSemana))
                          .ThenBy(h => h.HoraInicio)
                          .ToList()
                );

                 // 👇 DICCIONARIO DE DOCENTES
    ViewBag.Docentes = _context.Usuarios
        .ToDictionary(u => u.Id, u => u.Nombre);

            return View("~/Views/Public/Horarios/Index.cshtml", horariosAgrupados);
        }

        // 🔧 ORDEN NATURAL DE DÍAS
        private int ObtenerOrdenDia(string dia)
{
    return dia?.ToLower() switch
    {
        "lunes" => 1,
        "martes" => 2,
        "miércoles" or "miercoles" => 3,
        "jueves" => 4,
        "viernes" => 5,
        _ => 99   // cualquier otro día queda al final
    };
}


        // 📣 LISTADO DE EVENTOS PÚBLICOS
        public IActionResult Eventos()
        {
            var eventos = _context.Eventos
                .Where(e => e.Publicado)
                .OrderByDescending(e => e.Fecha)
                .ToList();

            return View("~/Views/Public/Eventos/Index.cshtml", eventos);
        }

        // 📘 DETALLE DE EVENTO
        public IActionResult Detalle(int id)
        {
            var evento = _context.Eventos
                .Where(e => e.Publicado && e.Id == id)
                .FirstOrDefault();

            if (evento == null)
                return NotFound();

            return View("~/Views/Public/Eventos/Detalle.cshtml", evento);
        }
    }
}
