using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
    public class Noticia
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string DescripcionCorta { get; set; } = string.Empty;

        [Required]
        public string Contenido { get; set; } = string.Empty;

        public string? ImagenUrl { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public bool Publicada { get; set; } = false;
    }
}
