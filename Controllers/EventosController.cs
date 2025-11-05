using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class EventosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EventosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private bool EsDocenteOAdmin()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Docente" || rol == "Admin";
        }

        // 📋 Index: muestra según el rol
        public IActionResult Index()
        {
            var eventos = EsDocenteOAdmin()
                ? _context.Eventos.OrderByDescending(e => e.Fecha).ToList()        // todos
                : _context.Eventos.Where(e => e.Publicado).OrderByDescending(e => e.Fecha).ToList(); // solo publicados

            return View(eventos);
        }

        // 🟢 Crear evento
        public IActionResult Crear()
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            ViewBag.Info = _context.InformacionInstitucional.FirstOrDefault();
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Evento model, IFormFile? Imagen)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            if (!ModelState.IsValid)
                return View(model);

            // 📸 Guardar imagen si se sube
            if (Imagen != null && Imagen.Length > 0)
            {
                string carpeta = Path.Combine(_env.WebRootPath, "uploads/eventos");
                Directory.CreateDirectory(carpeta);

                string fileName = Guid.NewGuid() + Path.GetExtension(Imagen.FileName);
                string filePath = Path.Combine(carpeta, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Imagen.CopyTo(stream);
                }

                model.ImagenUrl = "/uploads/eventos/" + fileName;
            }

            model.DocenteId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            _context.Eventos.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 🟡 Editar evento
        public IActionResult Editar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var evento = _context.Eventos.Find(id);
            if (evento == null) return NotFound();

            return View(evento);
        }

        [HttpPost]
        public IActionResult Editar(Evento model, IFormFile? NuevaImagen)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var evento = _context.Eventos.Find(model.Id);
            if (evento == null) return NotFound();

            // Actualizar campos
            evento.Titulo = model.Titulo;
            evento.Descripcion = model.Descripcion;
            evento.Fecha = model.Fecha;
            evento.Publicado = model.Publicado;

            // 📸 Si sube una nueva imagen, reemplazar
            if (NuevaImagen != null && NuevaImagen.Length > 0)
            {
                string carpeta = Path.Combine(_env.WebRootPath, "uploads/eventos");
                Directory.CreateDirectory(carpeta);

                string fileName = Guid.NewGuid() + Path.GetExtension(NuevaImagen.FileName);
                string filePath = Path.Combine(carpeta, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    NuevaImagen.CopyTo(stream);
                }

                evento.ImagenUrl = "/uploads/eventos/" + fileName;
            }

            _context.Eventos.Update(evento);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 🔁 Publicar o despublicar
        [HttpPost]
        public IActionResult CambiarEstado(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var evento = _context.Eventos.Find(id);
            if (evento == null) return NotFound();

            evento.Publicado = !evento.Publicado;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 🔴 Eliminar evento
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var evento = _context.Eventos.Find(id);
            if (evento == null) return NotFound();

            _context.Eventos.Remove(evento);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
