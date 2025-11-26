using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{


public class HorarioCard
{
    public int Id { get; set; }

    public string Seccion { get; set; } = "";
    public string Grado { get; set; } = "";
    public string Grupo { get; set; } = ""; // A, B

    public string ImagenUrl { get; set; } = "";
    public DateTime FechaSubida { get; set; } = DateTime.Now;

    public int SubidoPorId { get; set; }
    public string SubidoPorNombre { get; set; } = "";
    public string RolSubidoPor { get; set; } = ""; // Admin / Docente
}
}
