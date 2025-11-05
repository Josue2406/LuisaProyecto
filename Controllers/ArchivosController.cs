using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
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
        public async Task<IActionResult> Subir(IFormFile archivo, string descripcion)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            if (archivo == null || archivo.Length == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar un archivo para subir.");
                return View();
            }

            // 📂 Carpeta donde se guardarán los archivos
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "archivos");
            Directory.CreateDirectory(uploadsFolder);

            // 🧾 Generar nombre único
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(fileStream);
            }

            // 🧠 Guardar registro en la base de datos
            var nuevo = new Archivo
            {
                Nombre = archivo.FileName,
                Descripcion = descripcion,
                Ruta = uniqueFileName, // ✅ Guardamos solo el nombre
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

            string rutaFisica = Path.Combine(_env.WebRootPath, "uploads", "archivos", archivo.Ruta);
            if (System.IO.File.Exists(rutaFisica))
                System.IO.File.Delete(rutaFisica);

            _context.Archivos.Remove(archivo);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
