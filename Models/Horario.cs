using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
    public class Horario
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Materia { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string DiaSemana { get; set; } = string.Empty;

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        // Relación con el docente
        public int DocenteId { get; set; }
        public Usuario? Docente { get; set; }

        public bool Publicado { get; set; } = true;
    }
}
