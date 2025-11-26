using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class PublicNoticiasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicNoticiasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTAR NOTICIAS PUBLICADAS
        public IActionResult Index()
        {
            var noticias = _context.Noticias
                .Where(n => n.Publicada)
                .OrderByDescending(n => n.Fecha)
                .ToList();

            return View("~/Views/Public/Noticias/Index.cshtml", noticias);
        }

        // DETALLE
        public IActionResult Detalle(int id)
        {
            var noticia = _context.Noticias
                .Where(n => n.Publicada && n.Id == id)
                .FirstOrDefault();

            if (noticia == null)
                return NotFound();

            return View("~/Views/Public/Noticias/Detalle.cshtml", noticia);
        }
    }
}
