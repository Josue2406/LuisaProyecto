using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class ArchivosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ArchivosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private bool EsDocenteOAdmin()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Docente" || rol == "Admin";
        }

        public IActionResult Index()
        {
            var archivos = _context.Archivos.OrderByDescending(a => a.FechaSubida).ToList();
            ViewBag.EsDocenteOAdmin = EsDocenteOAdmin();
            return View(archivos);
        }

        public IActionResult Subir()
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Subir(IFormFile archivo, string descripcion)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            if (archivo == null || archivo.Length == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un archivo para subir.");
                return View();
            }

            // Carpeta donde se guardarán
            string carpeta = Path.Combine(_env.WebRootPath, "uploads/archivos");
            Directory.CreateDirectory(carpeta);

            // Generar nombre único
            string fileName = Guid.NewGuid() + Path.GetExtension(archivo.FileName);
            string rutaArchivo = Path.Combine(carpeta, fileName);

            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            var nuevo = new Archivo
            {
                Nombre = archivo.FileName,
                Descripcion = descripcion,
                Ruta = "/uploads/archivos/" + fileName,
                FechaSubida = DateTime.Now,
                SubidoPor = HttpContext.Session.GetString("Usuario") ?? "Desconocido"
            };

            _context.Archivos.Add(nuevo);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var archivo = _context.Archivos.Find(id);
            if (archivo == null)
                return NotFound();

            string rutaFisica = Path.Combine(_env.WebRootPath, archivo.Ruta.TrimStart('/'));
            if (System.IO.File.Exists(rutaFisica))
                System.IO.File.Delete(rutaFisica);

            _context.Archivos.Remove(archivo);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
