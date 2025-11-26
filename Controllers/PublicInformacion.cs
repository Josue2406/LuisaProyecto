using Microsoft.AspNetCore.Mvc;

namespace ProyectoLuisa.Controllers.PublicInformacion
{
    public class PublicInformacionController : Controller
    {
        public IActionResult Mision()
        {
            return View();
        }

        public IActionResult Vision()
        {
            return View();
        }
    }
}
