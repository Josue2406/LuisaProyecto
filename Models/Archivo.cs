using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
    public class Archivo
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descripcion { get; set; }

        [Required]
        public string Ruta { get; set; } = string.Empty;

        public DateTime FechaSubida { get; set; } = DateTime.Now;

        [Required, StringLength(100)]
        public string SubidoPor { get; set; } = string.Empty;
    }
}
