using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Models;
using ProyectoLuisa.Data;

namespace ProyectoLuisa.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // 🔹 Cargar la información institucional
            var info = _context.InformacionInstitucional.FirstOrDefault();
            ViewBag.Info = info;

            // 🔹 Mostrar los últimos eventos publicados
            var eventos = _context.Eventos
                .Where(e => e.Publicado)
                .OrderByDescending(e => e.Fecha)
                .Take(3)
                .ToList();

            // 🔹 Mostrar horarios públicos
            var horarios = _context.Horarios
                .Where(h => h.Publicado)
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .Take(5)
                .ToList();

            ViewBag.Eventos = eventos;
            ViewBag.Horarios = horarios;

            return View();
        }

        public IActionResult Privacy()
        {
            var info = _context.InformacionInstitucional.FirstOrDefault();
            ViewBag.Info = info;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var info = _context.InformacionInstitucional.FirstOrDefault();
            ViewBag.Info = info;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
