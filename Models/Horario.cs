using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
  using System.ComponentModel.DataAnnotations;


  public class Horario
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Materia { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string DiaSemana { get; set; } = string.Empty;

    [Required]
    public TimeSpan HoraInicio { get; set; }

    [Required]
    public TimeSpan HoraFin { get; set; }

    [Required, StringLength(50)]
    public string Aula { get; set; } = string.Empty;

    // 🔹 Sección estándar: "1° - Mañana"
    [Required, StringLength(30)]
    public string Seccion { get; set; } = string.Empty;

    // 🔹 Id del docente (desde Session)
    public int DocenteId { get; set; }
}

}

