using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Controllers
{
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }


        /*
                public IActionResult Index()
                {
                    var roles = _context.Usuarios
                        .Select(u => u.Rol)
                        .Distinct()
                        .OrderBy(r => r)
                        .ToList();

                    ViewBag.Roles = roles;
                    return View();
                } */
        // 📋 Mostrar lista de roles activos e inactivos
        public IActionResult Index()
        {
            var roles = _context.Usuarios
                .GroupBy(u => u.Rol)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Activo = g.Any(u => u.Activo) // Si al menos un usuario con ese rol está activo, se considera activo
                })
                .OrderBy(r => r.Nombre)
                .ToList();

            ViewBag.Roles = roles;
            return View();
        }

        /*
                [HttpPost]
                public IActionResult Crear(string nombreRol)
                {
                    if (string.IsNullOrWhiteSpace(nombreRol))
                    {
                        TempData["Error"] = "El nombre del rol no puede estar vacío.";
                        return RedirectToAction("Index");
                    }

                    var existe = _context.Usuarios.Any(u => u.Rol == nombreRol);
                    if (existe)
                    {
                        TempData["Error"] = "Ese rol ya existe.";
                        return RedirectToAction("Index");
                    }

                    var fake = new Usuario
                    {
                        Nombre = "Rol-" + nombreRol,
                        Correo = $"{nombreRol}@placeholder.local",
                        Rol = nombreRol,
                        Activo = false
                    };

                    _context.Usuarios.Add(fake);
                    _context.SaveChanges();

                    TempData["Mensaje"] = $"Rol '{nombreRol}' creado exitosamente.";
                    return RedirectToAction("Index");
                } */

[HttpPost]
public IActionResult Crear(string nombreRol)
{
    if (string.IsNullOrWhiteSpace(nombreRol))
    {
        TempData["Error"] = "El nombre del rol no puede estar vacío.";
        return RedirectToAction("Index");
    }

    nombreRol = nombreRol.Trim();

    // ✅ Limitar solo a roles permitidos
    var rolesPermitidos = new[] { "Admin", "Docente" };

    if (!rolesPermitidos.Contains(nombreRol))
    {
        TempData["Error"] = $"Solo puedes crear roles válidos: {string.Join(", ", rolesPermitidos)}.";
        return RedirectToAction("Index");
    }

    // Evitar duplicados
    bool existe = _context.Usuarios.Any(u => u.Rol == nombreRol);
    if (existe)
    {
        TempData["Error"] = $"El rol '{nombreRol}' ya existe.";
        return RedirectToAction("Index");
    }

    // Crear usuario placeholder (solo para representar el rol)
    var placeholder = new Usuario
    {
        Nombre = $"Rol-{nombreRol}",
        Correo = $"{nombreRol}@placeholder.local",
        Rol = nombreRol,
        Activo = false
    };

    _context.Usuarios.Add(placeholder);
    _context.SaveChanges();

    TempData["Mensaje"] = $"Rol '{nombreRol}' creado correctamente.";
    return RedirectToAction("Index");
}




        // 🟡 EDITAR ROL (GET)
        public IActionResult Editar(string nombreRol)
        {
            if (string.IsNullOrEmpty(nombreRol)) return RedirectToAction("Index");
            ViewBag.RolActual = nombreRol;
            return View();
        }

        // 🟢 EDITAR ROL (POST)
        [HttpPost]
        public IActionResult Editar(string rolActual, string nuevoRol)
        {
            if (string.IsNullOrWhiteSpace(nuevoRol))
            {
                TempData["Error"] = "El nombre del nuevo rol no puede estar vacío.";
                return RedirectToAction("Index");
            }

            if (rolActual == "Admin")
            {
                TempData["Error"] = "El rol 'Admin' no puede ser modificado.";
                return RedirectToAction("Index");
            }

            // Actualizar a todos los usuarios que tengan el rol actual
            var usuarios = _context.Usuarios.Where(u => u.Rol == rolActual).ToList();
            if (usuarios.Any())
            {
                foreach (var u in usuarios)
                    u.Rol = nuevoRol;

                _context.SaveChanges();
            }

            TempData["Mensaje"] = $"Rol '{rolActual}' actualizado a '{nuevoRol}' correctamente.";
            return RedirectToAction("Index");
        }
 /*
        [HttpPost]
        public IActionResult Eliminar(string nombreRol)
        {
            if (nombreRol == "Admin")
            {
                TempData["Error"] = "No puedes eliminar el rol Admin.";
                return RedirectToAction("Index");
            }

            var usuarios = _context.Usuarios.Where(u => u.Rol == nombreRol).ToList();

            if (usuarios.Any())
            {
                foreach (var u in usuarios)
                {
                    u.Activo = false;
                }
            }

            var placeholders = _context.Usuarios
                .Where(u => u.Correo.EndsWith("@placeholder.local") && u.Rol == nombreRol);

            _context.Usuarios.RemoveRange(placeholders);
            _context.SaveChanges();

            TempData["Mensaje"] = $"Rol '{nombreRol}' eliminado y usuarios desactivados.";
            return RedirectToAction("Index");
        }
*/

         // 🟢 Desactivar rol
        [HttpPost]
        public IActionResult Desactivar(string nombreRol)
        {
            if (nombreRol == "Admin")
            {
                TempData["Error"] = "No puedes desactivar el rol Admin.";
                return RedirectToAction("Index");
            }

            var usuarios = _context.Usuarios.Where(u => u.Rol == nombreRol).ToList();
            if (usuarios.Any())
            {
                foreach (var u in usuarios)
                {
                    u.Activo = false;
                }
                _context.SaveChanges();
            }

            TempData["Mensaje"] = $"Rol '{nombreRol}' desactivado. Ningún usuario con ese rol podrá iniciar sesión.";
            return RedirectToAction("Index");
        }

        // 🗑️ Eliminar rol (definitivo)
        [HttpPost]
        public IActionResult Eliminar(string nombreRol)
        {
            if (nombreRol == "Admin")
            {
                TempData["Error"] = "No puedes eliminar el rol Admin.";
                return RedirectToAction("Index");
            }

            var usuarios = _context.Usuarios.Where(u => u.Rol == nombreRol).ToList();

            // Desactivar usuarios y eliminar placeholders del rol
            if (usuarios.Any())
            {
                foreach (var u in usuarios)
                    u.Activo = false;
            }

            var placeholders = _context.Usuarios
                .Where(u => u.Correo.EndsWith("@placeholder.local") && u.Rol == nombreRol);

            _context.Usuarios.RemoveRange(placeholders);
            _context.SaveChanges();

            TempData["Mensaje"] = $"Rol '{nombreRol}' eliminado completamente y usuarios desactivados.";
            return RedirectToAction("Index");
        }


        [HttpPost]
public IActionResult Reactivar(string nombreRol)
{
    if (string.IsNullOrEmpty(nombreRol))
    {
        TempData["Error"] = "Rol no especificado.";
        return RedirectToAction("Index");
    }

    var usuarios = _context.Usuarios.Where(u => u.Rol == nombreRol).ToList();

    if (!usuarios.Any())
    {
        TempData["Error"] = $"No se encontraron usuarios con el rol '{nombreRol}'.";
        return RedirectToAction("Index");
    }

    foreach (var u in usuarios)
    {
        u.Activo = true;
    }

    _context.SaveChanges();

    TempData["Mensaje"] = $"Rol '{nombreRol}' reactivado correctamente. Los usuarios pueden iniciar sesión nuevamente.";
    return RedirectToAction("Index");
}

    

    }
    
    
}
