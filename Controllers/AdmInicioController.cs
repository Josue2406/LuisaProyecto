using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data; // Ajusta según tu proyecto
using System.Linq;

namespace ProyectoLuisa.Controllers
{
    public class AdmInicioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdmInicioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Cargar estadísticas
            ViewBag.TotalDocentes = _context.Usuarios.Count(u => u.Rol == "Docente");
            ViewBag.TotalNoticias = _context.Noticias.Count();
            ViewBag.TotalEventos = _context.Eventos.Count();
            ViewBag.TotalArchivos = _context.Archivos.Count();

            return View();
        }
    }
}
