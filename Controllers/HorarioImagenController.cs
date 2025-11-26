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

    public IActionResult Crear()
    {
        return View(new HorarioImagen());
    }

    [HttpPost]
    public IActionResult Crear(HorarioImagen model, IFormFile imagen)
    {
        if (!ModelState.IsValid)
            return View(model);

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

    public IActionResult Editar(int id)
    {
        var h = _context.HorarioImagenes.Find(id);
        if (h == null) return NotFound();

        return View(h);
    }

    [HttpPost]
public IActionResult Editar(HorarioImagen model, IFormFile? imagen)
{
    // Validación de campos requeridos
    if (!ModelState.IsValid)
    {
        // Opcional: imprimir errores en consola para debug
        var errores = ModelState.Values.SelectMany(v => v.Errors);
        foreach (var e in errores)
            Console.WriteLine(e.ErrorMessage);

        // Retorna la vista con los datos ingresados para corregir
        return View(model);
    }

    // Buscar el registro en la base de datos
    var h = _context.HorarioImagenes.Find(model.Id);
    if (h == null)
        return NotFound();

    // Actualizar solo los campos de texto
    h.Grado = model.Grado;
    h.Seccion = model.Seccion;
    h.Docente = model.Docente;

    // Reemplazar imagen solo si se subió una nueva
    if (imagen != null && imagen.Length > 0)
    {
        // Eliminar imagen anterior si existe
        if (!string.IsNullOrEmpty(h.ImagenUrl))
        {
            string rutaVieja = Path.Combine(_env.WebRootPath, h.ImagenUrl.TrimStart('/'));
            if (System.IO.File.Exists(rutaVieja))
                System.IO.File.Delete(rutaVieja);
        }

        // Guardar nueva imagen
        string archivo = Guid.NewGuid() + Path.GetExtension(imagen.FileName);
        string carpetaRel = Path.Combine("uploads/horarios", archivo);
        string rutaFisica = Path.Combine(_env.WebRootPath, carpetaRel);

        Directory.CreateDirectory(Path.GetDirectoryName(rutaFisica)!);

        using var stream = new FileStream(rutaFisica, FileMode.Create);
        imagen.CopyTo(stream);

        h.ImagenUrl = "/" + carpetaRel.Replace("\\", "/");
    }

    _context.SaveChanges();

    // Redirigir al índice después de guardar
    return RedirectToAction("Index");
}


    [HttpPost]
    public IActionResult Eliminar(int id)
    {
        var h = _context.HorarioImagenes.Find(id);
        if (h == null) return NotFound();

        // Eliminar archivo físico
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
}
