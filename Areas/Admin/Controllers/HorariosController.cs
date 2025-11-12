using Microsoft.AspNetCore.Mvc;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;
using System;

namespace ProyectoLuisa.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HorariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HorariosController(ApplicationDbContext context) => _context = context;

        // 🔒 Validar rol
        private bool EsDocenteOAdmin()
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rol))
                return false;

            rol = rol.Trim().ToLower();
            return rol == "admin" || rol == "administrador" || rol == "docente" || rol == "profesor";
        }

        // ✅ Validar rango horario permitido
        private bool HorarioValido(TimeSpan inicio, TimeSpan fin)
        {
            var rango1Inicio = new TimeSpan(7, 0, 0);
            var rango1Fin = new TimeSpan(16, 30, 0);
            var rango2Inicio = new TimeSpan(17, 0, 0);
            var rango2Fin = new TimeSpan(22, 0, 0);

            return (inicio >= rango1Inicio && fin <= rango1Fin) ||
                   (inicio >= rango2Inicio && fin <= rango2Fin);
        }

        // ✅ Validar conflictos (aula y profesor)
        private bool TieneConflicto(Horario h, int? ignoreId, out string mensaje)
        {
            var dia = (h.DiaSemana ?? "").Trim();
            var aula = (h.Aula ?? "").Trim();
            var prof = (h.Profesor ?? "").Trim();

            if (h.HoraInicio >= h.HoraFin)
            {
                mensaje = "❌ La hora de inicio no puede ser igual o posterior a la hora de finalización.";
                return true;
            }

            // Conflicto de aula
            bool conflictoAula = _context.Horarios.Any(x =>
                (ignoreId == null || x.Id != ignoreId) &&
                x.DiaSemana == dia &&
                x.Aula == aula &&
                (
                    (h.HoraInicio >= x.HoraInicio && h.HoraInicio < x.HoraFin) ||
                    (h.HoraFin > x.HoraInicio && h.HoraFin <= x.HoraFin) ||
                    (h.HoraInicio <= x.HoraInicio && h.HoraFin >= x.HoraFin)
                )
            );

            if (conflictoAula)
            {
                mensaje = $"❌ El aula {aula} ya está ocupada ese día y hora.";
                return true;
            }

            // Conflicto de profesor
            bool conflictoProfesor = _context.Horarios.Any(x =>
                (ignoreId == null || x.Id != ignoreId) &&
                x.DiaSemana == dia &&
                x.Profesor == prof &&
                (
                    (h.HoraInicio >= x.HoraInicio && h.HoraInicio < x.HoraFin) ||
                    (h.HoraFin > x.HoraInicio && h.HoraFin <= x.HoraFin) ||
                    (h.HoraInicio <= x.HoraInicio && h.HoraFin >= x.HoraFin)
                )
            );

            if (conflictoProfesor)
            {
                mensaje = $"❌ El profesor {prof} ya tiene una clase asignada en ese horario.";
                return true;
            }

            mensaje = string.Empty;
            return false;
        }

        // 🧭 INDEX (lista de horarios ordenados)
        public IActionResult Index(string? dia, string? grupo, string? profesor)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var q = _context.Horarios.AsQueryable();

            if (!string.IsNullOrEmpty(dia))
                q = q.Where(h => h.DiaSemana.Contains(dia));

            if (!string.IsNullOrEmpty(grupo))
                q = q.Where(h => h.Grupo.Contains(grupo));

            if (!string.IsNullOrEmpty(profesor))
                q = q.Where(h => h.Profesor.Contains(profesor));

            // 🗓️ Ordenar por día y hora
            int OrderDia(string d) => (d ?? "").ToLower() switch
            {
                "lunes" => 1,
                "martes" => 2,
                "miércoles" or "miercoles" => 3,
                "jueves" => 4,
                "viernes" => 5,
                "sábado" or "sabado" => 6,
                _ => 7
            };

            var lista = q.AsEnumerable()
                         .OrderBy(h => OrderDia(h.DiaSemana))
                         .ThenBy(h => h.HoraInicio)
                         .ToList();

            ViewBag.EsDocenteOAdmin = true;
            return View(lista);
        }

        // 🟢 Crear (GET)
        public IActionResult Crear()
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            return View();
        }

        // 🟢 Crear (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Horario model)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            if (!ModelState.IsValid)
                return View(model);

            if (!HorarioValido(model.HoraInicio, model.HoraFin))
            {
                TempData["Error"] = "⚠️ Los horarios deben estar entre 7:00 a.m.–4:30 p.m. o 5:00 p.m.–10:00 p.m.";
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

        // ✏️ Editar (GET)
        public IActionResult Editar(int id)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            var horario = _context.Horarios.Find(id);
            if (horario == null)
                return NotFound();

            return View(horario);
        }

        // ✏️ Editar (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Horario model)
        {
            if (!EsDocenteOAdmin())
                return View("~/Views/Shared/AccesoDenegado.cshtml");

            if (!ModelState.IsValid)
                return View(model);

            if (!HorarioValido(model.HoraInicio, model.HoraFin))
            {
                TempData["Error"] = "⚠️ Los horarios deben estar entre 7:00 a.m.–4:30 p.m. o 5:00 p.m.–10:00 p.m.";
                return View(model);
            }

            if (TieneConflicto(model, model.Id, out var msg))
            {
                TempData["Error"] = msg;
                return View(model);
            }

            _context.Update(model);
            _context.SaveChanges();

            TempData["Success"] = "✅ Horario actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // 🗑️ Eliminar
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

            TempData["Success"] = "🗑️ Horario eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
