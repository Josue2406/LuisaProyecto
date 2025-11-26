
using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{


public class HorarioImagen
{
    public int Id { get; set; }

    [Range(1, 6, ErrorMessage = "El grado debe ser entre 1 y 6.")]
    public int Grado { get; set; } // 1° a 6°

    [Required]
    public string Seccion { get; set; } = ""; // A, B, C...

    [Required]
    public string Docente { get; set; } = "";

    public string ImagenUrl { get; set; } = "";
    public string SubidoPor { get; set; } = "";
    public DateTime FechaSubida { get; set; } = DateTime.Now;
}
}