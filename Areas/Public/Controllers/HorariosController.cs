using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;

namespace ProyectoLuisa.Areas.Public.Controllers
{
    [Area("Public")]
    public class HorariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HorariosController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult Index(string? seccionSeleccionada, bool verTodos = false)
        {
            var secciones = _context.Horarios
                .Where(h => !string.IsNullOrEmpty(h.Seccion))
                .Select(h => h.Seccion!.Trim())
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            ViewBag.Secciones = secciones;
            ViewBag.SeccionSeleccionada = seccionSeleccionada;
            ViewBag.VerTodos = verTodos;

            var horarios = _context.Horarios.AsQueryable();
            if (!string.IsNullOrEmpty(seccionSeleccionada))
                horarios = horarios.Where(h => h.Seccion == seccionSeleccionada);

            var agrupados = horarios
                .GroupBy(h => h.Seccion)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio).ToList()
                );

            if (verTodos)
            {
                agrupados = _context.Horarios
                    .Where(h => !string.IsNullOrEmpty(h.Seccion))
                    .GroupBy(h => h.Seccion)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio).ToList()
                    );
            }

            return View(agrupados); // Areas/Public/Views/Horarios/Index.cshtml
        }
    }
}
