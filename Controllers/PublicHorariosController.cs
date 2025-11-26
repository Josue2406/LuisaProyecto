using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;

namespace ProyectoLuisa.Controllers.PublicHorarios
{
public class PublicHorariosController : Controller
{
    private readonly ApplicationDbContext _context;

    public PublicHorariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var horarios = _context.HorarioImagenes
            .OrderBy(h => h.Grado)
            .ThenBy(h => h.Seccion)
            .ToList();

        return View("~/Views/Public/PublicHorarios/Index.cshtml", horarios);

    }

    public IActionResult Detalle(int id)
    {
        var h = _context.HorarioImagenes.Find(id);
        if (h == null) return NotFound();

        return View("~/Views/Public/PublicHorarios/Detalle.cshtml", h);
;
    }
}
}