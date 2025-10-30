using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class InformacionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InformacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mostrar información
        public IActionResult Index()
        {
            var info = _context.InformacionInstitucional.FirstOrDefault();
            if (info == null)
            {
                info = new InformacionInstitucional();
                _context.InformacionInstitucional.Add(info);
                _context.SaveChanges();
            }

            return View(info);
        }

        // Guardar cambios
        [HttpPost]
        public IActionResult Guardar(InformacionInstitucional model)
        {
            var info = _context.InformacionInstitucional.FirstOrDefault();
            if (info == null)
            {
                _context.InformacionInstitucional.Add(model);
            }
            else
            {
                info.Mision = model.Mision;
                info.Vision = model.Vision;
                info.Valores = model.Valores;
                info.Telefono = model.Telefono;
                info.Correo = model.Correo;
                info.Direccion = model.Direccion;
                info.LogoUrl = model.LogoUrl;
                info.FooterTexto = model.FooterTexto;
            }

            _context.SaveChanges();
            TempData["Mensaje"] = "Información institucional actualizada correctamente.";
            return RedirectToAction("Index");
        }
    }
}
