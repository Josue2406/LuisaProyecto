using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;

namespace ProyectoLuisa.Controllers.PublicInformacion
{
    public class PublicInformacionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicInformacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Mision()
        {
            var info = _context.InformacionInstitucional.FirstOrDefault();
            return View(info);
        }

        public IActionResult Vision()
        {
            var info = _context.InformacionInstitucional.FirstOrDefault();
            return View(info);
        }
    }
}
