using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
    public class Evento
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime Fecha { get; set; }

        // Relación con el docente que lo creó
        public int DocenteId { get; set; }
        public Usuario? Docente { get; set; }

        // Público o no
        public bool Publicado { get; set; } = true;
    }
}
