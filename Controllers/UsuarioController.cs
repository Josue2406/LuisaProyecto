using Microsoft.AspNetCore.Mvc;

namespace ProyectoLuisa.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Inicio()
        {
            return View();
        }
    }
}
