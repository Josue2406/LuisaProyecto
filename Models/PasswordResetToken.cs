using System.ComponentModel.DataAnnotations;

namespace ProyectoLuisa.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;


        public DateTime Expira { get; set; } = DateTime.UtcNow.AddHours(24); // 24h para invitación

        

    }
}
