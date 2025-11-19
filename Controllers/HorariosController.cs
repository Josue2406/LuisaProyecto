using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;
using System;

namespace ProyectoLuisa.Controllers
{
    public class HorariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HorariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔒 Validar rol
        private bool EsDocenteOAdmin()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Admin" || rol == "Administrador" || rol == "Docente" || rol == "Profesor";
        }

        // 🔢 Orden natural de días
        private int ObtenerOrdenDia(string dia)
        {
            return dia?.ToLower() switch
            {
                "lunes" => 1,
                "martes" => 2,
                "miércoles" or "miercoles" => 3,
                "jueves" => 4,
                "viernes" => 5,
                _ => 99 // cualquier otro día queda al final
            };
        }

        // ⚠ Validar rango (mañana/tarde)
        private bool HorarioValido(TimeSpan inicio, TimeSpan fin)
        {
            var rango1Inicio = new TimeSpan(7, 0, 0);
            var rango1Fin = new TimeSpan(16, 30, 0);
            var rango2Inicio = new TimeSpan(17, 0, 0);
            var rango2Fin = new TimeSpan(22, 0, 0);

            return (inicio >= rango1Inicio && fin <= rango1Fin)
                || (inicio >= rango2Inicio && fin <= rango2Fin);
        }

        // ⚠ Validación de conflictos
        private bool TieneConflicto(Horario h, int? ignoreId, out string mensaje)
        {
            var dia = (h.DiaSemana ?? "").Trim();

            if (h.HoraInicio >= h.HoraFin)
            {
                mensaje = "❌ La hora de inicio no puede ser igual o posterior a la hora final.";
                return true;
            }

            // 🔹 AULA OCUPADA
            bool conflictoAula = _context.Horarios.Any(x =>
                (ignoreId == null || x.Id != ignoreId) &&
                x.DiaSemana.Trim() == dia &&
                x.Aula.Trim() == h.Aula.Trim() &&
                (
                    (h.HoraInicio >= x.HoraInicio && h.HoraInicio < x.HoraFin) ||
                    (h.HoraFin > x.HoraInicio && h.HoraFin <= x.HoraFin) ||
                    (h.HoraInicio <= x.HoraInicio && h.HoraFin >= x.HoraFin)
                )
            );

            if (conflictoAula)
            {
                mensaje = $"❌ El aula {h.Aula} ya está ocupada en ese horario.";
                return true;
            }

            // 🔹 PROFESOR OCUPADO
            bool conflictoProfe = _context.Horarios.Any(x =>
                (ignoreId == null || x.Id != ignoreId) &&
                x.DiaSemana.Trim() == dia &&
                x.DocenteId == h.DocenteId &&
                (
                    (h.HoraInicio >= x.HoraInicio && h.HoraInicio < x.HoraFin) ||
                    (h.HoraFin > x.HoraInicio && h.HoraFin <= x.HoraFin) ||
                    (h.HoraInicio <= x.HoraInicio && h.HoraFin >= x.HoraFin)
                )
            );

            if (conflictoProfe)
            {
                var profe = _context.Usuarios
                    .Where(u => u.Id == h.DocenteId)
                    .Select(u => u.Nombre)
                    .FirstOrDefault() ?? "El docente";

                mensaje = $"❌ {profe} ya tiene una clase en ese horario.";
                return true;
            }

            mensaje = "";
            return false;
        }

        // 📋 INDEX
        public IActionResult Index(string? dia, string? seccion, int? docenteId)
        {
            if (!EsDocenteOAdmin())
                return RedirectToAction("Index", "PublicHorarios");

            var horarios = _context.Horarios.AsQueryable();

            if (!string.IsNullOrEmpty(dia))
                horarios = horarios.Where(h => h.DiaSemana.Contains(dia));

            if (!string.IsNullOrEmpty(seccion))
                horarios = horarios.Where(h => h.Seccion.Contains(seccion));

            if (docenteId.HasValue)
                horarios = horarios.Where(h => h.DocenteId == docenteId.Value);

            var listaOrdenada = horarios
                .AsEnumerable()
                .OrderBy(h => ObtenerOrdenDia(h.DiaSemana))
                .ThenBy(h => h.HoraInicio)
                .ToList();

            var docentesDict = _context.Usuarios
                .Where(u => listaOrdenada.Select(h => h.DocenteId).Distinct().Contains(u.Id))
                .ToDictionary(
                    u => u.Id,
                    u => $"{u.Nombre} ({u.Rol})"
                );

            ViewBag.Docentes = docentesDict;
            ViewBag.EsDocenteOAdmin = EsDocenteOAdmin();

            return View(listaOrdenada);
        }

        // 🟢 Crear GET
        public IActionResult Crear()
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            ViewBag.Secciones = new List<string>
            {
                "1° - Mañana","1° - Tarde",
                "2° - Mañana","2° - Tarde",
                "3° - Mañana","3° - Tarde",
                "4° - Mañana","4° - Tarde",
                "5° - Mañana","5° - Tarde",
                "6° - Mañana","6° - Tarde"
            };

            return View();
        }

        // 🟢 Crear POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Horario model)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            // ⛔ Escuela solo lunes a viernes
            if (model.DiaSemana == "Sábado" || model.DiaSemana == "Sabado")
            {
                TempData["Error"] = "⚠️ La escuela solo trabaja de lunes a viernes.";
                return View(model);
            }

            model.DocenteId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            if (!HorarioValido(model.HoraInicio, model.HoraFin))
            {
                TempData["Error"] = "⚠️ El horario debe ser entre 7am–4:30pm o 5pm–10pm.";
                return View(model);
            }

            if (TieneConflicto(model, null, out var msg))
            {
                TempData["Error"] = msg;
                return View(model);
            }

            _context.Horarios.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "✅ Horario registrado correctamente.";
            return RedirectToAction("Index");
        }

        // ✏ Editar GET
        public IActionResult Editar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var horario = _context.Horarios.Find(id);
            if (horario == null)
                return NotFound();

            ViewBag.Secciones = new List<string>
            {
                "1° - Mañana","1° - Tarde",
                "2° - Mañana","2° - Tarde",
                "3° - Mañana","3° - Tarde",
                "4° - Mañana","4° - Tarde",
                "5° - Mañana","5° - Tarde",
                "6° - Mañana","6° - Tarde"
            };

            return View(horario);
        }

        // ✏ Editar POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Horario model)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            // ⛔ Escuela solo lunes a viernes
            if (model.DiaSemana == "Sábado" || model.DiaSemana == "Sabado")
            {
                TempData["Error"] = "⚠️ La escuela solo trabaja de lunes a viernes.";
                return View(model);
            }

            model.DocenteId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            if (!HorarioValido(model.HoraInicio, model.HoraFin))
            {
                TempData["Error"] = "⚠️ El horario debe ser entre 7am–4:30pm o 5pm–10pm.";
                return View(model);
            }

            if (TieneConflicto(model, model.Id, out var msg))
            {
                TempData["Error"] = msg;
                return View(model);
            }

            _context.Update(model);
            _context.SaveChanges();

            TempData["Success"] = "✅ Horario actualizado.";
            return RedirectToAction("Index");
        }

        // 🗑 Eliminar
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var horario = _context.Horarios.Find(id);
            if (horario == null)
                return NotFound();

            _context.Horarios.Remove(horario);
            _context.SaveChanges();

            TempData["Success"] = "🗑 Horario eliminado.";
            return RedirectToAction("Index");
        }
    }
}
