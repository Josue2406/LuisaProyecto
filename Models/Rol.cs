using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Relación con usuarios
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}
