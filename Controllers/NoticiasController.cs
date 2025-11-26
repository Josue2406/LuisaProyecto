using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class NoticiasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public NoticiasController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // LISTADO ADMIN
        public IActionResult Index()
        {
            var noticias = _context.Noticias
                .OrderByDescending(n => n.Fecha)
                .ToList();

            return View(noticias);
        }

        // CREAR GET
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Noticia model, IFormFile? imagen)
        {
            if (!ModelState.IsValid)
                return View(model);

            // SUBIR IMAGEN
            if (imagen != null)
            {
                string carpeta = Path.Combine(_env.WebRootPath, "uploads/noticias");
                Directory.CreateDirectory(carpeta);

                string nombre = Guid.NewGuid() + Path.GetExtension(imagen.FileName);
                string ruta = Path.Combine(carpeta, nombre);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }

                model.ImagenUrl = "/uploads/noticias/" + nombre;
            }

            _context.Noticias.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Noticia creada correctamente.";
            return RedirectToAction("Index");
        }

        // EDITAR GET
        public IActionResult Editar(int id)
        {
            var noticia = _context.Noticias.Find(id);
            if (noticia == null) return NotFound();

            return View(noticia);
        }

        // EDITAR POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Noticia model, IFormFile? imagen)
        {
            var original = _context.Noticias.AsNoTracking()
                                .FirstOrDefault(n => n.Id == model.Id);

            if (original == null) return NotFound();

            // SUBIR NUEVA IMAGEN
            if (imagen != null)
            {
                string carpeta = Path.Combine(_env.WebRootPath, "uploads/noticias");
                Directory.CreateDirectory(carpeta);

                string nombre = Guid.NewGuid() + Path.GetExtension(imagen.FileName);
                string ruta = Path.Combine(carpeta, nombre);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }

                model.ImagenUrl = "/uploads/noticias/" + nombre;
            }
            else
            {
                model.ImagenUrl = original.ImagenUrl;
            }

            _context.Noticias.Update(model);
            _context.SaveChanges();

            TempData["Success"] = "Noticia actualizada correctamente.";
            return RedirectToAction("Index");
        }

        // ELIMINAR
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var noticia = _context.Noticias.Find(id);
            if (noticia == null) return NotFound();

            _context.Noticias.Remove(noticia);
            _context.SaveChanges();

            TempData["Success"] = "Noticia eliminada.";
            return RedirectToAction("Index");
        }
    }
}
