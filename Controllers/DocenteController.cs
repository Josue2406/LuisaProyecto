using Microsoft.AspNetCore.Mvc;

namespace ProyectoLuisa.Controllers
{
    public class DocenteController : Controller
    {
        /* public IActionResult Inicio()
         {
             return View();
         } */


 // 🔐 Verifica que exista una sesión activa
        private bool SesionActiva()
        {
            return HttpContext.Session.GetInt32("UsuarioId") != null &&
                   (HttpContext.Session.GetString("Rol") == "Docente" ||
                    HttpContext.Session.GetString("Rol") == "Admin");
        }

        public IActionResult Inicio()
        {
            // Si no hay sesión → redirigir al acceso denegado o Home
            if (!SesionActiva())
                return View("~/Views/Shared/AccesoDenegado.cshtml"); // RedirectToAction("Index","Home")

            // Si hay sesión, muestra el panel del docente
            ViewBag.Nombre = HttpContext.Session.GetString("Nombre");
            return View();
        }
    }


    }

