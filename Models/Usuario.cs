using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = "Usuario"; // Usuario o Docente

        public string ContrasenaHash { get; set; } = string.Empty;

        public bool Activo { get; set; } = false; // Se activa con el link

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
