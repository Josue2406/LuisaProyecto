using Microsoft.EntityFrameworkCore;
using ProyectoLuisa.Data;
using ProyectoLuisa.Models;

namespace ProyectoLuisa.Services
{
    public class HorarioService
    {
        private readonly ApplicationDbContext _context;

        public HorarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🧠 Validación profesional
        public async Task<string?> ValidarConflictosAsync(Horario nuevo)
        {
            // 1️⃣ Validar hora coherente
            if (nuevo.HoraInicio >= nuevo.HoraFin)
                return "❌ La hora de inicio no puede ser igual o posterior a la hora de finalización.";

            // 2️⃣ Validar conflicto de aula
            var conflictoAula = await _context.Horarios.AnyAsync(h =>
                h.Id != nuevo.Id && // evitar conflicto consigo mismo en edición
                h.DiaSemana == nuevo.DiaSemana &&
                h.Aula == nuevo.Aula &&
                (
                    (nuevo.HoraInicio >= h.HoraInicio && nuevo.HoraInicio < h.HoraFin) ||
                    (nuevo.HoraFin > h.HoraInicio && nuevo.HoraFin <= h.HoraFin) ||
                    (nuevo.HoraInicio <= h.HoraInicio && nuevo.HoraFin >= h.HoraFin)
                )
            );

            if (conflictoAula)
                return $"❌ El aula {nuevo.Aula} ya está ocupada ese día y hora.";

            // 3️⃣ Validar conflicto de profesor
            var conflictoProfesor = await _context.Horarios.AnyAsync(h =>
                h.Id != nuevo.Id &&
                h.DiaSemana == nuevo.DiaSemana &&
                h.Profesor == nuevo.Profesor &&
                (
                    (nuevo.HoraInicio >= h.HoraInicio && nuevo.HoraInicio < h.HoraFin) ||
                    (nuevo.HoraFin > h.HoraInicio && nuevo.HoraFin <= h.HoraFin) ||
                    (nuevo.HoraInicio <= h.HoraInicio && nuevo.HoraFin >= h.HoraFin)
                )
            );

            if (conflictoProfesor)
                return $"❌ El profesor {nuevo.Profesor} ya tiene una clase asignada en ese horario.";

            return null; // todo correcto
        }

        public async Task<(bool exito, string mensaje)> GuardarHorarioAsync(Horario horario)
        {
            var validacion = await ValidarConflictosAsync(horario);
            if (validacion != null)
                return (false, validacion);

            _context.Horarios.Add(horario);
            await _context.SaveChangesAsync();
            return (true, "✅ Horario registrado correctamente.");
        }
    }
}
