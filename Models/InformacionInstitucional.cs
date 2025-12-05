using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
    public class InformacionInstitucional
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(500)]
        public string Mision { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Vision { get; set; } = string.Empty;

      /*  [StringLength(500)]
        public string Valores { get; set; } = string.Empty; */

        [StringLength(500)]
public string Historia { get; set; } = string.Empty;

        [StringLength(100)]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(100)]
        public string Correo { get; set; } = string.Empty;

        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [StringLength(200)]
        public string LogoUrl { get; set; } = string.Empty;

        [StringLength(300)]
        public string FooterTexto { get; set; } = string.Empty;
    }
}
