using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

public class HorarioImagenController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public HorarioImagenController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IActionResult Index()
    {
        var lista = _context.HorarioImagenes
            .OrderByDescending(x => x.FechaSubida)
            .ToList();

        return View(lista);
    }

    // -------------------- Crear --------------------
    public IActionResult Crear()
    {
        CargarDatosParaSelects();
        return View(new HorarioImagen());
    }

    [HttpPost]
    public IActionResult Crear(HorarioImagen model, IFormFile imagen)
    {
        if (!ModelState.IsValid)
        {
            CargarDatosParaSelects();
            return View(model);
        }

        if (imagen != null && imagen.Length > 0)
        {
            string archivo = Guid.NewGuid() + Path.GetExtension(imagen.FileName);
            string carpetaRel = Path.Combine("uploads/horarios", archivo);
            string rutaFisica = Path.Combine(_env.WebRootPath, carpetaRel);

            Directory.CreateDirectory(Path.GetDirectoryName(rutaFisica));

            using var stream = new FileStream(rutaFisica, FileMode.Create);
            imagen.CopyTo(stream);

            model.ImagenUrl = "/" + carpetaRel.Replace("\\", "/");
        }

        model.SubidoPor = HttpContext.Session.GetString("Nombre") ?? "Desconocido";
        model.FechaSubida = DateTime.Now;

        _context.HorarioImagenes.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // -------------------- Editar --------------------
    public IActionResult Editar(int id)
    {
        var h = _context.HorarioImagenes.Find(id);
        if (h == null) return NotFound();

        CargarDatosParaSelects();
        return View(h);
    }

    [HttpPost]
    public IActionResult Editar(HorarioImagen model, IFormFile? imagen)
    {
        if (!ModelState.IsValid)
        {
            CargarDatosParaSelects();
            return View(model);
        }

        var h = _context.HorarioImagenes.Find(model.Id);
        if (h == null) return NotFound();

        h.Grado = model.Grado;
        h.Seccion = model.Seccion;
        h.Docente = model.Docente;

        if (imagen != null && imagen.Length > 0)
        {
            if (!string.IsNullOrEmpty(h.ImagenUrl))
            {
                string rutaVieja = Path.Combine(_env.WebRootPath, h.ImagenUrl.TrimStart('/'));
                if (System.IO.File.Exists(rutaVieja))
                    System.IO.File.Delete(rutaVieja);
            }

            string archivo = Guid.NewGuid() + Path.GetExtension(imagen.FileName);
            string carpetaRel = Path.Combine("uploads/horarios", archivo);
            string rutaFisica = Path.Combine(_env.WebRootPath, carpetaRel);

            Directory.CreateDirectory(Path.GetDirectoryName(rutaFisica)!);

            using var stream = new FileStream(rutaFisica, FileMode.Create);
            imagen.CopyTo(stream);

            h.ImagenUrl = "/" + carpetaRel.Replace("\\", "/");
        }

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    // -------------------- Eliminar --------------------
    [HttpPost]
    public IActionResult Eliminar(int id)
    {
        var h = _context.HorarioImagenes.Find(id);
        if (h == null) return NotFound();

        if (!string.IsNullOrEmpty(h.ImagenUrl))
        {
            string ruta = Path.Combine(_env.WebRootPath, h.ImagenUrl.TrimStart('/'));
            if (System.IO.File.Exists(ruta))
                System.IO.File.Delete(ruta);
        }

        _context.HorarioImagenes.Remove(h);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // -------------------- Método auxiliar para cargar selects --------------------
    private void CargarDatosParaSelects()
    {
        // Grados 1 a 6
        ViewBag.Grados = Enumerable.Range(1, 6).Select(g => g.ToString()).ToList();

        // Secciones fijas
        ViewBag.Secciones = new List<string> { "A", "B", "C", "D" };

        // Docentes registrados
        ViewBag.Docentes = _context.Usuarios
                            .Where(u => u.Rol == "Docente")
                            .Select(u => u.Nombre)
                            .ToList();
    }
}
